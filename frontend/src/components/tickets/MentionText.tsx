import { tokenizeMentions } from '@/utils/mentions'

export function MentionText({ content }: { content: string }) {
  const tokens = tokenizeMentions(content)

  return (
    <span className="whitespace-pre-wrap">
      {tokens.map((token, index) =>
        token.type === 'mention' ? (
          <span key={index} className="rounded bg-primary/10 px-1 font-medium text-primary">
            @{token.value}
          </span>
        ) : (
          <span key={index}>{token.value}</span>
        ),
      )}
    </span>
  )
}
