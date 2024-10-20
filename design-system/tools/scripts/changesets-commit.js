/**
 * Functions to create Git commit message when a changelog is added
 *
 * Modification of #getAddMessage from @changelog/cli
 * https://github.com/changesets/changesets/blob/main/packages/cli/src/commit/index.ts
 */
async function getAddMessage(changeset, _options) {
  return `${changeset.summary || 'Added empty changelog'}`
}

/**
 * Max number of changelog to be included in release commit
 * to prevent clogging up git log
 */
const maxChangelogDisplay = 3

/**
 * Functions to create Git commit message for a version bump.
 * NOTE: This commit is also used as text message for automatically
 * notifying about a release after version bump in the publish
 * pipeline. If emoji is used, please ensure that hexcode is used
 * to prevent BAD REQUEST in release notification.
 *
 * Modification of #getVersionMessage from @changelog/cli
 * https://github.com/changesets/changesets/blob/main/packages/cli/src/commit/index.ts
 */
async function getVersionMessage(releasePlan, _options) {
  const publishableReleases = releasePlan.releases.filter(
    (release) => release.type !== 'none'
  )
  const filteredChangeset = releasePlan.changesets.filter(
    (changeset) => changeset.summary
  )
  const numPackagesReleased = publishableReleases.length

  const messages = [
    // Commit Subject
    'Platform UI Release',
    '',
    // Commit Body
    `Releasing ${numPackagesReleased} package(s):`,
    ...publishableReleases.map(
      (release) => `  - ${release.name}@${release.newVersion}`
    ),
    '',
    'Changelog:',
    ...filteredChangeset
      .slice(0, maxChangelogDisplay)
      .map((changeset) => `  - ${changeset.summary}`),
  ]

  if (filteredChangeset.length > maxChangelogDisplay) {
    messages.push(`  - ... and more great stuff ...`)
  }

  return messages.join('\n')
}

module.exports = {
  getAddMessage,
  getVersionMessage,
}
