# AI Sloppers · Fallback Plan

1. **Model call:** `PurchaseProductAsync` through the Lab 8 fallback router. If models are unavailable, the user receives a clear degraded reply and no purchase action is claimed as completed.
2. **Chain:** `google/gemini-3.8-flash`, then `anthropic/claude-haiku-5.5`. Gemini Flash is first; Claude Haiku is a fallback from another company.
3. **Limit:** `TIMEOUT_S = 12` seconds.
4. **Lifetime:** `CACHE_TTL_S = 600` seconds. Plain chat answers may be reused for ten minutes; tool calls are never cached.
5. **Sorry sentence:** "We cannot reach our models right now. Nothing was changed. Please try again in a minute."
