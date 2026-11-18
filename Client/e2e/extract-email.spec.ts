import { expect, test } from '@playwright/test'
import { sampleEmail } from '../src/sampleEmail'

// Restore the line breaks inside the marked fields in the assessment PDF.
const assessmentEmail = sampleEmail
  .replace('personal card', 'personal\ncard')
  .replace('27 April 2022', '27 April\n2022')

test('extracts the assessment email through the UI and real API', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('textbox', { name: 'Email text' }).fill(assessmentEmail)
  await page.getByRole('button', { name: 'Submit', exact: true }).click()

  const output = page.getByLabel('Extracted JSON', { exact: true })
  await expect(output).toBeVisible()
  const result: unknown = JSON.parse(await output.innerText())
  expect(result).toEqual({
    extractedFields: {
      expense: {
        costCentre: 'DEV632',
        total: '35,000',
        paymentMethod: 'personal\ncard',
      },
      vendor: 'Seaside Steakhouse',
      description: 'development team’s project end celebration',
      date: '27 April\n2022',
    },
    calculation: {
      total: 35000,
      salesTax: 4565.22,
      totalExcludingTax: 30434.78,
      taxRate: 0.15,
    },
    warnings: [],
  })
})
