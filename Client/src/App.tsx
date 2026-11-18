import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { extractEmail } from './extractEmail'
import { sampleEmail } from './sampleEmail'

type Submission =
  | { status: 'idle' }
  | { status: 'pending' }
  | { status: 'success'; json: string; warnings: string[] }
  | { status: 'error'; message: string }

export function App() {
  const [email, setEmail] = useState(sampleEmail)
  const [submission, setSubmission] = useState<Submission>({ status: 'idle' })
  const activeRequest = useRef<AbortController | null>(null)
  const emailInput = useRef<HTMLTextAreaElement>(null)

  useEffect(() => () => activeRequest.current?.abort(), [])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!email.trim() || activeRequest.current) {
      return
    }

    const request = new AbortController()
    activeRequest.current = request
    setSubmission({ status: 'pending' })

    try {
      const result = await extractEmail(email, request.signal)
      if (!request.signal.aborted) {
        setSubmission({ status: 'success', json: JSON.stringify(result, null, 2), warnings: result.warnings })
      }
    } catch (error) {
      if (!request.signal.aborted) {
        const message = error instanceof Error ? error.message : 'Extraction failed.'
        setSubmission({ status: 'error', message })
      }
    } finally {
      if (activeRequest.current === request) {
        activeRequest.current = null
      }
    }
  }

  function updateEmail(value: string) {
    activeRequest.current?.abort()
    activeRequest.current = null
    setEmail(value)
    setSubmission({ status: 'idle' })
  }

  function clear() {
    updateEmail('')
    emailInput.current?.focus()
  }

  return (
    <main className="workspace">
      <header className="page-header">
        <h1>Email Extractor</h1>
        <p>Extract tagged fields and calculate sales tax from input text.</p>
      </header>

      <form className="workbench" onSubmit={submit}>
        <div className="panel email-panel">
          <label className="panel-heading" htmlFor="email-text">Email text</label>
          <textarea
            id="email-text"
            ref={emailInput}
            value={email}
            onChange={(event) => updateEmail(event.target.value)}
            placeholder="Paste your email here…"
            rows={12}
          />
        </div>

        <section className="panel" aria-labelledby="result-heading">
          <h2 className="panel-heading" id="result-heading">JSON output</h2>
          <div className="panel-content output">
            {submission.status === 'success' && (
              <pre tabIndex={0} aria-label="Extracted JSON"><code>{submission.json}</code></pre>
            )}
            {submission.status === 'idle' && (
              <p className="output-message">Submit an email to see the extracted JSON.</p>
            )}
          </div>
        </section>

        <section className="panel" aria-labelledby="validation-heading">
          <h2 className="panel-heading" id="validation-heading">Validation</h2>
          <div className="panel-content validation" aria-live="polite">
            {submission.status === 'pending' && (
              <p className="output-message">Validating…</p>
            )}
            {submission.status === 'success' && submission.warnings.length === 0 && (
              <p className="output-message validation-success">All good</p>
            )}
            {submission.status === 'success' && submission.warnings.length > 0 && (
              <ul className="validation-warnings">
                {submission.warnings.map((warning, index) => <li key={index}>{warning}</li>)}
              </ul>
            )}
            {submission.status === 'error' && (
              <ul className="validation-errors">
                <li>{submission.message}</li>
              </ul>
            )}
          </div>
        </section>

        <div className="actions">
          <button
            type="submit"
            className="primary-button"
            disabled={!email.trim() || submission.status === 'pending'}
          >
            Submit
          </button>
          <button type="button" onClick={clear}>Clear</button>
        </div>
      </form>
    </main>
  )
}
