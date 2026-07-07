# Development Notes

A narrative retrospective on building this project phase by phase — what went wrong, how it was diagnosed, and what it taught. For the complete, itemized technical log this summarizes, see [`PITFALLS.md`](PITFALLS.md).

## Problems encountered and solutions

### "Is the caller unknown, or just not allowed here?" (Phase 3)
Early on, every authorization failure returned `401 Unauthorized` — including an Employee trying to view someone else's ticket, which is a *permission* problem, not an *authentication* problem. Live-testing that exact scenario surfaced the bug. The fix was splitting the exception hierarchy: `UnauthorizedAppException` (401, genuine auth failures) vs. a new `ForbiddenAppException` (403, authenticated but not permitted). **Lesson:** manually walking through a permission boundary as the "wrong" user catches bugs that unit tests focused on the "right" user's happy path don't.

### Duplicate dev server instances causing silent CORS failures (Phase 4)
Registration appeared to "do nothing" in the browser. The backend log showed the CORS preflight (`OPTIONS`) succeeding but the actual `POST` never arriving — which meant the browser was blocking the real request client-side. The cause: a second Vite dev server had started on port 5174 (the backend's CORS policy only allowlists 5173), and the browser tab had landed on the wrong port. **Lesson:** "the preflight succeeded but nothing happened" is a specific, recognizable signature of a CORS origin mismatch — check *which* origin actually made the request before assuming the backend is broken.

### Round-robin assignment state (Phase 4)
Auto-assignment needs to remember "who got the last ticket" to rotate fairly. The first instinct was a dedicated counter/state table. Simpler and equally correct: derive it from the assignment history itself — query the most recent `TicketAssignment` row of type `RoundRobin`. **Lesson:** before adding a new piece of state, check whether the answer is already derivable from data you're recording anyway.

### `SaveChanges` silently overwriting test fixture timestamps (Phase 5)
Dashboard tests needed tickets created across several different months, but every seeded ticket landed in the *same* month — `AppDbContext`'s audit-stamping override forces `CreatedAt = UtcNow` on every insert, regardless of what the test set beforehand. The fix: save once (so the entity is tracked), then set the desired historical `CreatedAt` and save again — on that second save the entity is `Modified`, not `Added`, and the override only touches `UpdatedAt` for modified rows. **Lesson:** a "helpful" global override can quietly defeat test fixtures in ways that only show up as wrong *aggregate* numbers, not a crash — worth checking early when a computed value looks suspiciously uniform.

### Third-party license gate (Phase 5)
QuestPDF throws at runtime unless a license type is selected globally. Setting it in `Program.cs` covered the running app but not the test suite, since xUnit never executes `Program.cs`. **Lesson:** anything a composition root sets up as a side effect (license flags, global settings) needs its own equivalent in the test host — don't assume test and production entry points share implicit setup.

### File locks during rebuilds (recurring, Phases 1–5)
Rebuilding the backend while `dotnet run` was still active from a previous verification pass reliably produced `MSB3026`/`MSB3027` "file is locked" errors. This became a routine, expected step: stop the running process → rebuild → restart → re-verify with a real HTTP request (not just trusting a "process started" message).

## Lessons learned

- **Live-testing the "wrong" user catches what unit tests miss.** Nearly every authorization bug found in this project (401-vs-403, internal-note visibility, mention-notification leakage) was caught by manually acting as the *unauthorized* party, not by a green test suite.
- **Derive state before you store state.** Round-robin rotation and SLA compliance both turned out to be computable from data already being recorded (assignment history, ticket due dates) rather than needing a new configuration table.
- **A clean `git log` isn't a substitute for documentation written *during* the work.** This project's `PITFALLS.md` was updated at the end of every phase, while the reasoning was still fresh — reconstructing it after the fact, as this file necessarily was, loses detail a contemporaneous note wouldn't.
- **"It didn't error" isn't verification.** Several bugs in this project (the CORS origin mismatch, the backend process silently dying between sessions) only surfaced because a request was actually sent and its real response checked — not because a command's exit code looked fine.
