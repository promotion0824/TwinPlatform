import { Preview } from '@storybook/react'

import { rootPreview } from '@design-system/.storybook/preview'

const preview: Preview = {
  ...rootPreview,
  parameters: {
    ...(rootPreview.parameters || {}),
    options: {
      ...((rootPreview.parameters && rootPreview.parameters['options']) || {}),
      storySort: {
        order: [
          'Welcome',
          'Getting Started',
          ['Designers', 'Developers'],
          'Design System',
          ['Color Palette', 'Color System', 'Iconography'],
          'Theme',
          ['Theme Package', 'Global Tokens', 'Base Tokens', 'Theme Tokens'],
          ['Alpha'],
          'Contributing',
          [
            'Developer Contributions Guide',
            'Getting Started',
            'Quality Control',
            'Adding Changelog',
            'Distribution',
            'Bugs And Requests',
          ],
          'Release Notes',
          ['Palette', 'Theme', 'UI', 'MUI Theme'],
          'WIP',
          'Components',
        ],
      },
    },
  },
}

export default preview
