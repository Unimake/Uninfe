---
name: redmine-issues-rules
description: Rules for generating Redmine issue tickets from review findings.
---

## Objective

Generate actionable Redmine issue tickets when explicitly requested by the user (e.g., feature/bug/spike/technical-debt tickets), or by flows that explicitly call this file for ticket formatting (such as the `redmine-ticket-generation` skill).

This file is NOT used to spin off tickets from code review findings. Under `.ai/rules/redmine-review-rules.md`, non-blocking findings ("Achadinhos") are documented inline in the review itself and do not generate separate tickets, regardless of the final review status (`Aprovado` or `Reprovado`).

---

## Output Language

- The ticket content MUST be written in Brazilian Portuguese (pt-BR).
- Keep technical terms in English when needed for precision.
- The ticket content MUST be compatible with Redmine Textile markup.

---

## Ticket Generation Rules

- Generate ONE ticket per actionable issue.
- DO NOT group unrelated issues into a single ticket.
- Keep tickets objective and implementation-focused.
- Avoid generic descriptions.
- Include concrete technical context.
- Acceptance criteria must be testable and objective.
- Focus on measurable outcomes.
- Do not truncate or wrap generated lines; always output each line in full.
- All generated Redmine/Textile lines must start at column 1, with no leading spaces or left margin.
- For any bullet list in generated Redmine/Textile output, use `*` and never `-`.
- Whenever a Redmine/Textile header is used (for example `h3.`), the next content line must be separated by one blank line.
- Ticket writing, formatting, and structure rules belong in this file and must not depend on `.ai/rules/redmine-review-rules.md`.
- You MUST persist each generated ticket as a `.textile` file inside `.review` in the directory where the review is being executed.
- Use `.review` as the default folder name for persisted review and ticket artifacts.
- Persisted ticket files must use a unique name composed of ticket type, scope identifier, and timestamp to avoid collisions.

---

## Redmine Issue Template (MANDATORY)

Use EXACTLY the following structure:

Title: <TÍTULO AQUI>

[DESCRIÇÃO]

Como <tipo de usuário ou sistema>.
Quero <ação/correção esperada>.
Para que <resultado esperado ou risco mitigado>.

h3. Contexto técnico:

* <detalhes técnicos relevantes>
* <arquivo/classe/método afetado>
* <impacto técnico>

[CRITÉRIOS DE ACEITAÇÃO]

* Dado <estado inicial>; quando <ação>; então <resultado esperado>;
* Dado <estado inicial>; quando <ação>; então <resultado esperado>;

---

## Important Behavior

- Prefer concise and actionable tickets.
- Avoid implementation overengineering.
- Acceptance criteria must validate the expected behavior.
- Keep focus on the reported issue.
- Do not invent technical details when evidence is missing.
- Do not add leading spaces or left margin before any generated Redmine/Textile line.
- In Redmine/Textile lists, use `*` instead of `-` so Redmine renders the bullet list correctly when pasted.
- In [CRITÉRIOS DE ACEITAÇÃO], use `*` instead of `-`.
- Keep each generated line complete so it can be copied and pasted without broken wrapping.
- Before finalizing the ticket text, ensure no generated bullet line starts with `-`.
- Do not rely on formatting rules defined in `.ai/rules/redmine-review-rules.md`; this file is the authoritative source for ticket writing.
- Create the `.review` folder if it does not exist.
- Persist each ticket as raw Redmine/Textile content so the user can copy it into Redmine without chat formatting interference.
- Ticket filenames written under `.review` should follow the pattern `ticket-<ticket-type>-<scope-id>-<yyyyMMdd-HHmmss>.textile`.