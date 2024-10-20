/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

const delayedResponseOne = {
  responseText:
    'Hello! It looks like your message is incomplete. How can I assist you today? If you have any questions or need information on a specific topic, feel free to let me know!',
  citations: [
    { name: 'Sample-Page1.pdf', pages: ['Page1'] },
    { nme: 'Sample-Page2.pdf', pages: ['Page2'] },
    { name: 'Sample-page3.pdf', page: ['Page3'] },
    {
      name: 'context',
      pages: [],
    },
    {
      name: 'history',
      pages: [],
    },
  ],
}

const delayedResponseTwo = {
  responseText:
    'It seems like your messages consist of a question mark without additional context. If you have a specific question or topic youd like assistance with, please provide more details, and I will do my best to help!',
  citations: [
    { name: 'Sample-Page10.pdf', pages: ['Page1'] },
    { name: 'Sample-Page11.pdf', pages: ['Page2'] },
    { name: undefined, pages: ['Page3'] },
    { name: 'Sample-Page12.pdf' },
    { name: undefined, pages: undefined },
    {
      name: 'context',
      pages: [],
    },
    {
      name: 'history',
      pages: [],
    },
  ],
}

const delayedResponseThree = {
  responseText:
    'It seems like there might be some confusion. If you have a question or a topic youd like to discuss, please provide more information or clarify, and I will be happy to assist you!',
  citations: [
    { name: 'Sample-Page4.pdf', pages: ['Page1'] },
    { name: 'Sample-Page5.pdf', pages: ['Page2'] },
    { name: 'Sample-page6.pdf', pages: ['Page3'] },
    { name: undefined, pages: ['Page4'] },
    { name: 'Sample-Page7.pdf' },
    { name: undefined, pages: undefined },
    {
      name: 'context',
      pages: [],
    },
    {
      name: 'history',
      pages: [],
    },
  ],
}

const delayedResponseFour = {
  responseText:
    'It seems like there might be some confusion or a technical issue. If you have a question or a specific topic youd like assistance with, please provide more details, and I will do my best to help!',
  citations: [
    { name: 'Sample-Page7.pdf', pages: ['Page1'] },
    { name: 'Sample-Page8.pdf', pages: ['Page2'] },
    { name: 'Sample-page9.pdf', pages: ['Page3'] },
    { name: undefined, page: ['Page6'] },
    { name: 'Sample-Page15.pdf' },
    { name: undefined, page: undefined },
    {
      name: 'context',
      pages: [],
    },
    {
      name: 'history',
      pages: [],
    },
  ],
}

const delayedResponses = [
  delayedResponseOne,
  delayedResponseTwo,
  delayedResponseThree,
  delayedResponseFour,
  delayedResponseOne,
  delayedResponseTwo,
  delayedResponseThree,
  delayedResponseFour,
]

export const handlers = [
  rest.post('/:region/api/chat', (req, res, ctx) => {
    if (req.body.userInput === 'ERROR?') {
      return res(ctx.delay(4000), ctx.status(400))
    }

    const random = Math.floor(Math.random() * delayedResponses.length)
    const result = delayedResponses[random]

    return res(ctx.delay(4000), ctx.json(result))
  }),
]
