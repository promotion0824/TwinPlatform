import titleCase from './titleCase'

describe('titleCase', () => {
  test.each([
    {
      value: 'groupe : Règle',
      expected: 'Groupe : Règle',
      language: 'fr',
    },
    {
      value: 'règles',
      expected: 'Règles',
      language: 'fr',
    },
    {
      value: "vue d'étage",
      expected: "Vue d'Étage",
      language: 'fr',
    },
    {
      value: 'évitable par jour',
      expected: 'Évitable par Jour',
      language: 'fr',
    },
    {
      value: 'date de dernière occurrence',
      expected: 'Date de Dernière Occurrence',
      language: 'fr',
    },
    {
      value: 'résumé des Insights',
      expected: 'Résumé des Insights',
      language: 'fr',
    },
    {
      value: 'datE De Dernière OCCurrence',
      expected: 'Date de Dernière Occurrence',
      language: 'fr',
    },
    {
      value: "Une erreur s'est produite",
      expected: "Une Erreur s'est Produite",
      language: 'fr',
    },
    {
      value: 'éVitable paR Jour',
      expected: 'Évitable par Jour',
      language: 'fr',
    },
    {
      value: 'willow & co',
      expected: 'Willow & Co',
      language: 'en',
    },
    {
      value: 'coSt Per Year',
      expected: 'Cost per Year',
      language: 'en',
    },
    {
      value: 'aN erRor hAs OcCurred',
      expected: 'An Error Has Occurred',
      language: 'en',
    },
    {
      value: 'sumMary oF InSights',
      expected: 'Summary of Insights',
      language: 'en',
    },
    {
      value: 'avoidable impact per year',
      expected: 'Avoidable Impact per Year',
      language: 'en',
    },
    {
      value: 'total impact to date',
      expected: 'Total Impact to Date',
      language: 'en',
    },
  ])(
    'value of "$value" will be formatted to "$expected" when language is "$language"',
    async ({ value, expected, language }) => {
      expect(titleCase({ text: value, language })).toBe(expected)
    }
  )
})
