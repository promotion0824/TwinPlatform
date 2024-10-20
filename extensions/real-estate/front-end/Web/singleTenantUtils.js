const prompts = require('prompts')

const devInstances = [
  {
    title: 'CI',
    url: 'https://willow-ci.app-dev.willowinc.com',
  },
  {
    title: 'Willow FGA',
    url: 'https://willow-fga.app-dev.willowinc.com',
  },
  {
    title: 'Willow LT',
    url: 'https://willow-lt.app-dev.willowinc.com',
  },
  {
    title: 'Willow PPE',
    url: 'https://willow-ppe.app.willowinc.com',
  },
]

const prodInstances = [
  {
    title: 'AXA Investment Managers',
    url: 'https://axa.app.willowinc.com',
  },
  {
    title: 'BNP Paribas - Nuveen',
    url: 'https://bnp-nuv.app.willowinc.com',
  },
  {
    title: 'BP',
    url: 'https://bp.app.willowinc.com',
  },
  {
    title: 'Brookfield',
    url: 'https://brookfield.app.willowinc.com',
  },
  {
    title: 'Dallas Fort Worth Airport',
    url: 'https://dfw.app.willowinc.com',
  },
  {
    title: 'DDK',
    url: 'https://ddk.app.willowinc.com',
  },
  {
    title: 'Durst',
    url: 'https://durst.app.willowinc.com',
  },
  {
    title: 'Hollywood Park',
    url: 'https://hollywoodpark.app.willowinc.com',
  },
  {
    title: 'Investa',
    url: 'https://investa.app.willowinc.com',
  },
  {
    title: 'JP Morgan Chase',
    url: 'https://jpmc.app.willowinc.com',
  },
  {
    title: 'Microsoft',
    url: 'https://msft.app.willowinc.com',
  },
  {
    title: 'Northern Arizona University',
    url: 'https://northernarizonauniversity.app.willowinc.com',
  },
  {
    title: 'Oxford',
    url: 'https://oxford.app.willowinc.com',
  },
  {
    title: 'Sanofi',
    url: 'https://sanofi.app.willowinc.com',
  },
  {
    title: 'Turner',
    url: 'https://turner.app.willowinc.com',
  },
  {
    title: 'Walmart Corporate',
    url: 'https://walmart.app.willowinc.com',
  },
  {
    title: 'Walmart Retail',
    url: 'https://walmartretail.app.willowinc.com',
  },
  {
    title: 'WatermanCLARK',
    url: 'https://watermanclark.app.willowinc.com',
  },
]

const uatInstances = [
  {
    title: 'Brookfield UAT',
    url: 'https://brookfield-uat.app.willowinc.com',
  },
  {
    title: 'Dallas Fort Worth Airport UAT',
    url: 'https://dfw-uat.app.willowinc.com',
  },
  {
    title: 'Hollywood Park UAT',
    url: 'https://hollywoodpark-uat.app.willowinc.com',
  },
  {
    title: 'Investa UAT',
    url: 'https://walmartretail-uat.app.willowinc.com',
  },
  {
    title: 'Walmart UAT',
    url: 'https://walmart-uat.app.willowinc.com',
  },
]

async function selectInstance(message, app, includeLocalInstance = true) {
  const { value } = await prompts({
    type: 'select',
    name: 'value',
    message,
    choices: [
      ...(includeLocalInstance
        ? [
            {
              title: 'Local',
              url:
                app === 'platform'
                  ? 'https://localhost:5001'
                  : 'https://localhost:5002',
            },
          ]
        : []),
      ...devInstances,
      ...prodInstances,
      ...uatInstances,
    ].map(({ title, url }) => ({
      title,
      description: url,
      value: url,
    })),
    initial: 1,
  })

  if (app === 'mobile' && !value.includes('localhost')) {
    return `${value}/mobile-web`
  } else {
    return value
  }
}

module.exports = {
  selectInstance,
  devInstances,
  prodInstances,
  uatInstances,
}
