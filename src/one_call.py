"""One model call through OpenRouter."""
import os

from openai import OpenAI

_client = None


def one_call(messages: list[dict], tools: list[dict], timeout: float | None = None, model: str | None = None) -> dict:
    global _client
    if _client is None:
        _client = OpenAI(base_url="https://openrouter.ai/api/v1", api_key=os.environ["OPENROUTER_API_KEY"])
    api = _client.with_options(timeout=timeout, max_retries=0)
    reply = api.chat.completions.create(model=model, messages=messages, **({"tools": tools} if tools else {}))
    message = reply.choices[0].message
    calls = [{"id": c.id, "name": c.function.name, "arguments": c.function.arguments or "{}"}
             for c in (message.tool_calls or [])]
    spent = getattr(reply.usage, "cost", None) or 0.0
    return {"content": message.content, "tool_calls": calls, "usage": {"cost_usd": spent}}
