import { useRef, useState } from 'react'
import { getMentionQuery, insertMention, type MentionCandidate } from '@/utils/mentions'

interface MentionTextareaProps {
  id?: string
  value: string
  onChange: (value: string) => void
  candidates: MentionCandidate[]
  placeholder?: string
  rows?: number
}

/** A plain textarea that shows a candidate dropdown while typing "@name" and inserts a `@[Name](id)` token on selection. */
export function MentionTextarea({ id, value, onChange, candidates, placeholder, rows = 3 }: MentionTextareaProps) {
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const [query, setQuery] = useState<string | null>(null)

  const matches =
    query === null ? [] : candidates.filter((c) => c.name.toLowerCase().includes(query.toLowerCase())).slice(0, 5)

  const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = event.target.value
    onChange(newValue)
    setQuery(getMentionQuery(newValue, event.target.selectionStart))
  }

  const handleSelect = (candidate: MentionCandidate) => {
    const cursorPos = textareaRef.current?.selectionStart ?? value.length
    const { text, cursorPos: newCursorPos } = insertMention(value, cursorPos, candidate)
    onChange(text)
    setQuery(null)
    requestAnimationFrame(() => {
      textareaRef.current?.focus()
      textareaRef.current?.setSelectionRange(newCursorPos, newCursorPos)
    })
  }

  return (
    <div className="relative">
      <textarea
        id={id}
        ref={textareaRef}
        value={value}
        onChange={handleChange}
        placeholder={placeholder}
        rows={rows}
        className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
      />
      {query !== null && matches.length > 0 && (
        <ul className="absolute z-10 mt-1 w-56 rounded-md border border-border bg-background shadow-md">
          {matches.map((candidate) => (
            <li key={candidate.id}>
              <button
                type="button"
                className="block w-full px-3 py-1.5 text-left text-sm hover:bg-muted"
                onClick={() => handleSelect(candidate)}
              >
                {candidate.name}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
