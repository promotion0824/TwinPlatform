/**
 * Currently we just treat Activate Packs like tags that can be applied to
 * apps. The user can filter to view only the apps that are associated with an
 * activate pack. In future this will exist in a database somewhere. For now we
 * use app names to identify the apps for niceness and also because the same
 * app can have different IDs depending on whether we're in production or UAT.
 *
 * There are intentionally several empty activate packs. Product are sure they
 * want this.
 */
const activatePacks: Array<{
  id: string
  translationKey: string
  appNames: string[]
}> = [
  {
    id: 'activeEfficiency',
    translationKey: 'activatePack.activeEfficiency',
    appNames: [],
  },
  {
    id: 'assetConditioning',
    translationKey: 'activatePack.assetConditioning',
    appNames: ['Airthings', 'View Glass', 'View Sense'],
  },
  {
    id: 'aviation',
    translationKey: 'activatePack.aviation',
    appNames: [],
  },
  {
    id: 'energyOperations',
    translationKey: 'activatePack.energyOperations',
    appNames: [
      '24/7',
      '24/7 V2',
      'Angus Anywhere',
      'Bueno',
      'CopperTree',
      'Facilio',
      'HxGN EAM',
      'DFW ELS Maintenance Connection', // DFW only
      'SamFM',
      'Schneider Electric: EcoStruxure',
      'Schneider Electric: EcoStruxure - IOTEdge',
      'Siemens Desigo',
      'Veoci',
      'ViewMondo',
      'WebTMA', // DFW only
      'Badger (Beacon)',
    ],
  },
  {
    id: 'conveyance',
    translationKey: 'activatePack.conveyance',
    appNames: [],
  },
  {
    id: 'healthcare',
    translationKey: 'activatePack.healthcare',
    appNames: [],
  },
  {
    id: 'onsiteFoodPrep',
    translationKey: 'activatePack.onsiteFoodPrep',
    appNames: [],
  },
  {
    id: 'retail',
    translationKey: 'activatePack.retail',
    appNames: [],
  },
  {
    id: 'spatialGeometry',
    translationKey: 'activatePack.spatialGeometry',
    appNames: [],
  },
  {
    id: 'sustainability',
    translationKey: 'activatePack.sustainability',
    appNames: [
      'Arcadia',
      'UtiliVisor',
      'Energy Star',
      'RTE France',
      'Weatherbit',
    ],
  },
  {
    id: 'occupancy',
    translationKey: 'activatePack.occupancy',
    appNames: [
      'C•Cure 9000', // (there are two of these for some reason)
      'FACIT: Smart Count',
      'Gallagher',
      'Inner Range: Integriti',
      'VergeSense',
      'MS VergeSense',
      'Disruptive Technologies',
      'Yanzi',
      'XY Sense',
    ],
  },
]

export default activatePacks
