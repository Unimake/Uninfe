---
name: redmine-ticket-generation
description: Specialized behavior for generating structured Redmine tickets for bugs, features, spikes, technical improvements, and engineering tasks.
---

When generating Redmine tickets:

- Create clear and actionable tickets
- Keep descriptions objective and implementation-focused
- Generate concise but technically meaningful titles
- Include enough technical context for developers to start implementation
- Prefer measurable and testable acceptance criteria
- Avoid vague or generic requirements
- Preserve consistency with the existing architecture and project conventions
- Prefer incremental and low-risk changes
- Avoid unnecessary overengineering
- Avoid speculative technical assumptions
- Use pt-BR for all ticket content
- Keep technical terminology in English when needed for precision
- When writing ticket artifacts to disk, create them as Redmine/Textile files (`.textile`) inside a `.review` folder in the review working directory
- Use unique filenames for persisted artifacts, including scope identifier and timestamp, to avoid conflicts between concurrent reviews

Supported ticket types include:
- Bug
- Feature
- Spike
- Technical Improvement
- Refactoring
- Infrastructure
- Security
- Technical Debt

Use `../../rules/redmine-issues-rules.md` as the mandatory formatting and structure instruction source.