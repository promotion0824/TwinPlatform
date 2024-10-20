const packagePrefix = '@willowinc/'

/**
 * Get markdown link to work item on Azure Devops in Unified board.
 */
const getLinkToWorkItem = (ticketNumber) =>
  `[#${ticketNumber}](https://dev.azure.com/willowdev/Unified/_workitems/edit/${ticketNumber}/)`

/**
 * Get markdown link to release notes page
 */
const getLinkToReleaseNote = (dependency) => {
  const { name, newVersion } = dependency
  const pageName = name.replace(packagePrefix, '')
  const linkToPath = `/docs/release-notes-${pageName}--docs#${newVersion.replaceAll(
    '.',
    ''
  )}`

  return `[${dependency.name}@${dependency.newVersion}](${linkToPath})`
}

/**
 * Get markdown link to GitHub Commit ID
 */
const getLinkToCommit = (commitId) => {
  return `[\`${commitId}\`](https://github.com/WillowInc/TwinPlatform/commit/${commitId})`
}

/**
 * Functions to create changelog for a version update
 *
 * Modification of #getReleaseLine from @changelog/git
 * https://github.com/changesets/changesets/blob/main/packages/changelog-git/src/index.ts
 */
async function getReleaseLine(changeset) {
  const [firstLine, ...futureLines] = changeset.summary
    .split('\n')
    .map((l) => l.trimRight())

  const [scope, messageStart] = firstLine.includes(':')
    ? firstLine.split(':')
    : ['General', firstLine]
  let returnVal = `- **${scope}**: ${getLinkToCommit(
    changeset.commit
  )} ${messageStart}`

  if (futureLines.length > 0) {
    returnVal += `\n${futureLines.map((l) => `  ${l}`).join('\n')}`
  }

  return returnVal.replaceAll(/AB#[\d]+/gi, (match) =>
    getLinkToWorkItem(match.replace('AB#', ''))
  )
}

/**
 * Functions to create changelog for dependency update
 *
 * Modification of #getDependencyReleaseLine from @changelog/git
 * https://github.com/changesets/changesets/blob/main/packages/changelog-git/src/index.ts
 */
async function getDependencyReleaseLine(
  changesets,
  dependenciesUpdated,
  changelogOpts
) {
  if (dependenciesUpdated.length === 0) return ''

  const commitLinks = changesets
    .map((changeset) => getLinkToCommit(changeset.commit))
    .join(', ')

  const updatedDependenciesList = dependenciesUpdated.map(
    (dependency) => `  - ${getLinkToReleaseNote(dependency)}`
  )

  return `${[
    `- Updated dependencies ${commitLinks}`,
    ...updatedDependenciesList,
  ].join('\n')}`
}

module.exports = {
  getReleaseLine,
  getDependencyReleaseLine,
}
