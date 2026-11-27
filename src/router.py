"""Lab 8 fallback chain."""
from __future__ import annotations

import hashlib
import json
import os
import time
from pathlib import Path

from src.one_call import one_call

CHAINS = {
    "chat": ["google/gemini-3.8-flash", "anthropic/claude-haiku-5.5"],
}
TIMEOUT_S = 12
CACHE_TTL_S = 600
SORRY = "We cannot reach our models right now. Nothing was changed. Please try again in a minute."

CACHE: dict[str, tuple[float, dict]] = {}
LOG = Path(os.environ.get("ROUTER_LOG", "logs/router.jsonl"))


def temporary(error: Exception) -> bool:
    name = type(error).__name__
    return (
        "Timeout" in name
        or "Connection" in name
        or getattr(error, "status_code", None) in (408, 429, 500, 502, 503, 504)
    )


def complete(messages: list[dict], tools: list[dict] | None = None, job: str = "chat", call=one_call) -> dict:
    key = hashlib.sha256(json.dumps([job, messages], sort_keys=True).encode()).hexdigest()
    saved = CACHE.get(key)
    if saved and not tools and time.time() - saved[0] < CACHE_TTL_S:
        return note(job, "cache", saved[1], saved_usd=cost(saved[1]))
    for place, model in enumerate(CHAINS[job], start=1):
        started = time.perf_counter()
        try:
            answer = call(messages, tools or [], timeout=TIMEOUT_S, model=model)
        except Exception as error:
            if not temporary(error):
                raise
            note(job, "failed", model=model, place=place, why=type(error).__name__)
            continue
        if not tools:
            CACHE[key] = (time.time(), answer)
        return note(job, "ok", answer, model=model, place=place, cost_usd=cost(answer),
                    ms=int((time.perf_counter() - started) * 1000))
    return note(job, "exhausted", {"content": SORRY, "tool_calls": [], "degraded": True})


def cost(answer: dict) -> float:
    return (answer.get("usage") or {}).get("cost_usd", 0.0)


def note(job: str, outcome: str, answer: dict | None = None, **fields) -> dict | None:
    LOG.parent.mkdir(parents=True, exist_ok=True)
    with LOG.open("a") as f:
        f.write(json.dumps({"ts": round(time.time(), 3), "job": job, "outcome": outcome, **fields}) + "\n")
    return answer


def summary(path: Path | None = None) -> dict:
    path = path or LOG
    lines = [json.loads(x) for x in path.read_text().splitlines()] if path.exists() else []
    count = lambda outcome: sum(1 for x in lines if x["outcome"] == outcome)
    ok = sorted(x["ms"] for x in lines if x["outcome"] == "ok")
    return {
        "requests": count("ok") + count("cache") + count("exhausted"),
        "answered_by_first_model": sum(1 for x in lines if x["outcome"] == "ok" and x["place"] == 1),
        "answered_by_a_fallback": sum(1 for x in lines if x["outcome"] == "ok" and x["place"] > 1),
        "answered_from_cache": count("cache"),
        "sorry_message": count("exhausted"),
        "failed_attempts": count("failed"),
        "median_ms": ok[len(ok) // 2] if ok else None,
        "spent_usd": round(sum(x.get("cost_usd", 0) for x in lines), 6),
        "saved_by_cache_usd": round(sum(x.get("saved_usd", 0) for x in lines), 6),
    }


if __name__ == "__main__":
    for name, value in summary().items():
        print(f"{name:<26} {value}")
