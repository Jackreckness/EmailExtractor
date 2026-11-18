type ExtractionResult = {
  warnings: string[]
  extractedFields: Record<string, unknown>
  calculation: {
    total: number
    salesTax: number
    totalExcludingTax: number
    taxRate: number
  }
}

export async function extractEmail(
  text: string,
  signal: AbortSignal,
): Promise<ExtractionResult> {
  let response: Response

  try {
    response = await fetch('/api/extract', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text }),
      signal,
    })
  } catch (error) {
    if (signal.aborted) {
      throw error
    }

    throw new Error('Could not reach the extraction service. Try again.', { cause: error })
  }

  const payload: unknown = await response.json().catch(() => undefined)

  if (!response.ok) {
    const message = isObject(payload) && typeof payload.message === 'string'
      ? payload.message
      : `The extraction service returned an error (${response.status}).`
    throw new Error(message)
  }

  if (!isExtractionResult(payload)) {
    throw new Error('The extraction service returned an unexpected response.')
  }

  return payload
}

function isExtractionResult(payload: unknown): payload is ExtractionResult {
  return isObject(payload)
    && Array.isArray(payload.warnings)
    && payload.warnings.every(warning => typeof warning === 'string')
    && isObject(payload.extractedFields)
    && isObject(payload.calculation)
    && typeof payload.calculation.total === 'number'
    && typeof payload.calculation.salesTax === 'number'
    && typeof payload.calculation.totalExcludingTax === 'number'
    && typeof payload.calculation.taxRate === 'number'
}

function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
