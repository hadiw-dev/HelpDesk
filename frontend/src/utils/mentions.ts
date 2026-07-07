const MENTION_REGEX = /@\[([^\]]+)\]\(([0-9a-fA-F-]{36})\)/g

export interface MentionToken {
  type: 'text' | 'mention'
  value: string
  userId?: string
}

export interface MentionCandidate {
  id: string
  name: string
}

/** Splits comment content into plain-text and @-mention tokens for rendering with highlighting. */
export function tokenizeMentions(content: string): MentionToken[] {
  const tokens: MentionToken[] = []
  let lastIndex = 0

  for (const match of content.matchAll(MENTION_REGEX)) {
    const index = match.index ?? 0
    if (index > lastIndex) {
      tokens.push({ type: 'text', value: content.slice(lastIndex, index) })
    }
    tokens.push({ type: 'mention', value: match[1], userId: match[2] })
    lastIndex = index + match[0].length
  }

  if (lastIndex < content.length) {
    tokens.push({ type: 'text', value: content.slice(lastIndex) })
  }

  return tokens
}

/** Returns the partial "@query" text right before the cursor, or null if the cursor isn't mid-mention. */
export function getMentionQuery(text: string, cursorPos: number): string | null {
  const before = text.slice(0, cursorPos)
  const match = before.match(/@([^\s@]*)$/)
  return match ? match[1] : null
}

/** Replaces the partial "@query" before the cursor with a resolved `@[Name](id)` mention token. */
export function insertMention(
  text: string,
  cursorPos: number,
  candidate: MentionCandidate,
): { text: string; cursorPos: number } {
  const before = text.slice(0, cursorPos)
  const match = before.match(/@([^\s@]*)$/)
  if (!match) {
    return { text, cursorPos }
  }

  const start = cursorPos - match[0].length
  const token = `@[${candidate.name}](${candidate.id}) `
  const newText = text.slice(0, start) + token + text.slice(cursorPos)
  return { text: newText, cursorPos: start + token.length }
}
