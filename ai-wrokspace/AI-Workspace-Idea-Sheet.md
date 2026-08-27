# AI Workspace Idea Sheet

## 1. Core Idea

The idea is to build a personal **AI Workspace System** that allows AI tools like Codex CLI, ChatGPT, Cursor, Claude Code, Gemini CLI, or other agents to understand my context, projects, workflow, and priorities without needing to explain everything from zero every time.

Instead of using AI as a blank chat box, I want to use it as a structured assistant that can read a defined set of files before helping me.

The system is based on local Markdown files such as:

```text
AGENTS.md
DNA.md
MEMORY.md
CURRENT_CONTEXT.md
domains/
skills/
```

These files act as the source of truth for how the AI should work with me.

---

## 2. Main Problem

Every time I start a new AI chat or open a new coding agent session, I usually need to repeat the same context:

- Who I am
- What I am building
- What my current projects are
- What my workflow is
- What tools I use
- What rules the AI should follow
- What should not be changed
- What phase I am currently in

This creates several issues:

- Wasted time repeating context
- Inconsistent answers between chats
- AI forgetting important decisions
- AI mixing different projects together
- AI giving generic advice instead of project-specific help
- Long chats becoming messy and hard to continue
- Coding agents making changes without understanding the full workflow

The goal is to solve this by creating a persistent, file-based AI context system.

---

## 3. Main Goal

The goal is to create a local AI workspace where an AI agent can:

1. Read the main instruction file.
2. Understand who I am and how I work.
3. Understand my current priorities.
4. Load the correct project or domain context.
5. Use the right skill or workflow.
6. Help me plan, write, review, code, learn, or organize work with better continuity.

The system should work for coding and non-coding tasks.

---

## 4. Core Workflow

The general workflow is:

```text
Open the right folder
↓
Start the AI agent
↓
AI reads AGENTS.md
↓
AI reads only the relevant context files
↓
AI understands the task domain
↓
AI gives a focused answer or performs the task
↓
Important decisions are saved back to MEMORY.md or CURRENT_CONTEXT.md
```

For Codex CLI, the workflow looks like this:

```bash
cd ~/AI-Workspace
codex
```

Then the prompt can be:

```text
Read AGENTS.md first, then read DNA.md, MEMORY.md, and CURRENT_CONTEXT.md if relevant.

Help me continue my current AI workspace workflow.
Focus only on the next practical step.
Do not overengineer.
```

For a specific project:

```bash
cd ~/Projects/vehicle-rental-platform
codex
```

Then the prompt can be:

```text
Read AGENTS.md first.

We are working on one step only.
Use the relevant project docs.
Do not edit files until you explain the scope and plan.
Do not commit unless I explicitly ask.
```

---

## 5. Recommended Folder Structure

```text
AI-Workspace/
├── AGENTS.md
├── DNA.md
├── MEMORY.md
├── CURRENT_CONTEXT.md
├── domains/
│   ├── vehicle-rental-saas.md
│   ├── social-media-content.md
│   ├── learning.md
│   └── ai-workflow.md
└── skills/
    ├── planning.md
    ├── review.md
    ├── documentation.md
    ├── software-architecture.md
    ├── content-strategy.md
    └── learning-coach.md
```

---

## 6. File Roles

### `AGENTS.md`

The main router file.

Its role is to tell the AI:

- How to behave
- Which files to read
- How to choose the right domain
- How to avoid unnecessary work
- How to report results
- How to follow my workflow

This file should not contain everything. It should guide the AI to the correct files.

---

### `DNA.md`

The stable personal context file.

It contains long-term information about me, such as:

- Who I am
- My background
- My learning style
- My long-term goals
- My communication preferences
- My general working style
- The kind of answers I prefer

This file changes rarely.

---

### `MEMORY.md`

The active memory file.

It contains important decisions and facts that should not be forgotten, such as:

- Current projects
- Approved decisions
- Tooling choices
- Workflow rules
- Repeated preferences
- Things the AI should avoid doing again

This file is updated when important decisions happen.

---

### `CURRENT_CONTEXT.md`

The short-term focus file.

It contains what matters right now, such as:

- Current focus
- Current phase
- What is not the focus now
- Immediate next steps
- Temporary constraints

This file helps the AI avoid jumping to unrelated topics.

---

### `domains/`

This folder separates different areas of work.

Example domains:

- `vehicle-rental-saas.md`
- `social-media-content.md`
- `learning.md`
- `ai-workflow.md`

Each file contains the important context for one specific area.

This prevents the AI from mixing my software project, content system, and learning plan together.

---

### `skills/`

This folder contains reusable working modes.

Example skills:

- Planning
- Review
- Documentation
- Software architecture
- Content strategy
- Learning coach

A skill is not magic. It is a focused instruction file that tells the AI how to work in a specific mode.

---

## 7. Main Use Cases

## Use Case 1 — General Planning

When I want to plan my work, I open the AI workspace:

```bash
cd ~/AI-Workspace
codex
```

Then I ask:

```text
Read AGENTS.md, MEMORY.md, and CURRENT_CONTEXT.md.

Help me decide the next practical phase for my work.
Consider my current projects, but do not overcomplicate the plan.
```

Expected result:

- A focused phased plan
- Better continuity
- Less generic advice
- Clear next action

---

## Use Case 2 — Project Development

When I work on the vehicle rental SaaS:

```bash
cd ~/Projects/vehicle-rental-platform
codex
```

Then I ask:

```text
Read AGENTS.md first.

We are working on one implementation step only.
Read only the required docs and files.
Explain the scope before editing.
After editing, report changed files and test commands.
Do not commit unless I ask.
```

Expected result:

- Cleaner coding-agent behavior
- Less overengineering
- Better respect for project rules
- Better test and review flow
- Safer implementation

---

## Use Case 3 — Memory Updates

After an important session, I ask:

```text
Based on this session, update MEMORY.md and CURRENT_CONTEXT.md.

Rules:
- Keep MEMORY.md concise.
- Keep CURRENT_CONTEXT.md focused on what matters next.
- Do not duplicate old information.
- Show the diff before applying.
```

Expected result:

- The workspace stays current
- Important decisions are not lost
- Future AI sessions become more accurate

---

## Use Case 4 — Social Media Content

When working on content:

```bash
cd ~/AI-Workspace
codex
```

Prompt:

```text
Read AGENTS.md and domains/social-media-content.md.

I want to create one content idea.
Do not improve the whole content framework.
Only help me structure this idea into:
- angle
- hook
- carousel outline
- caption direction
```

Expected result:

- Better content consistency
- Less random AI writing
- Clear separation between content creation and content system improvement

---

## Use Case 5 — Learning

When planning study sessions:

```text
Read AGENTS.md, DNA.md, and domains/learning.md.

Create a 7-day learning plan that supports my current software engineering goals.
Prioritize practical learning connected to my projects.
Avoid unnecessary theory.
```

Expected result:

- Learning becomes connected to real projects
- Study plans become more realistic
- AI advice becomes less generic

---

## 8. Real Benefit

The real benefit is not the files themselves.

The real benefit is continuity.

This system helps me:

- Stop repeating the same context every time
- Make AI answers more consistent
- Keep project decisions documented
- Separate different domains clearly
- Use Codex as a local workspace assistant
- Improve coding-agent discipline
- Build a reusable workflow for planning, coding, content, and learning
- Own my AI memory as files instead of relying only on app memory

---

## 9. What This System Does Not Do

This system does not magically make AI perfect.

It still requires:

- Clear prompts
- Updated files
- Manual review
- Good project documentation
- Regular cleanup
- Human judgment

If the files become outdated, the AI output will also become outdated.

The system is only useful if it is maintained.

---

## 10. Implementation Phases

### Phase 1 — Build the Core Workspace

Create the base folder:

```text
AI-Workspace/
├── AGENTS.md
├── DNA.md
├── MEMORY.md
├── CURRENT_CONTEXT.md
└── domains/
```

Goal:

Create the minimum useful system without automation or connectors.

---

### Phase 2 — Add Domain Files

Create separate files for each major area:

```text
domains/
├── vehicle-rental-saas.md
├── social-media-content.md
├── learning.md
└── ai-workflow.md
```

Goal:

Keep context separated and easy to update.

---

### Phase 3 — Use Codex CLI as the Local Agent

Start using Codex inside the workspace:

```bash
cd ~/AI-Workspace
codex
```

Goal:

Use the workspace for planning, reviewing, summarizing, and updating context.

---

### Phase 4 — Apply the System to Real Projects

Add or improve `AGENTS.md` inside important project folders, such as:

```text
vehicle-rental-platform/
social-media-content/
learning/
```

Goal:

Make every major project readable and usable by AI agents.

---

### Phase 5 — Add Skills

Create reusable skill files:

```text
skills/
├── planning.md
├── review.md
├── documentation.md
├── software-architecture.md
├── content-strategy.md
└── learning-coach.md
```

Goal:

Give the AI specialized working modes.

---

### Phase 6 — Add Connectors Later

Only after the file system is stable, consider adding:

- GitHub
- Google Drive
- Gmail
- Calendar
- Vercel
- Slack

Goal:

Allow AI to access useful external services when needed.

---

### Phase 7 — Add Automation Later

After the system is reliable, create automation pipelines for repeated work.

Possible examples:

```text
Content idea
↓
Research
↓
Outline
↓
Draft
↓
Review
↓
Publish checklist
```

or:

```text
Weekly planning
↓
Review current context
↓
Check priorities
↓
Generate weekly plan
↓
Update memory
```

Goal:

Automate repeated workflows only after the context system is clean.

---

## 11. Current Recommended Starting Point

Start simple.

Create only this first:

```text
AI-Workspace/
├── AGENTS.md
├── DNA.md
├── MEMORY.md
├── CURRENT_CONTEXT.md
└── domains/
    ├── vehicle-rental-saas.md
    ├── social-media-content.md
    ├── learning.md
    └── ai-workflow.md
```

Do not start with automation, connectors, or many skills.

First make the AI understand the workspace.

Then improve the system based on actual usage.

---

## 12. Success Criteria

This project is successful if:

- I repeat less context in AI conversations
- Codex gives more accurate and relevant answers
- AI follows my workflow more consistently
- Project decisions are easier to recover
- Each domain has clear context
- I can continue work after long breaks
- My learning, coding, and content work become more organized
- AI becomes part of my workflow instead of a random chat tool

---

## 13. One-Sentence Summary

This project is a personal AI workspace that uses local Markdown files to give AI agents stable context, clear instructions, memory, domain knowledge, and reusable workflows so they can help me more accurately across coding, learning, content, and planning.
