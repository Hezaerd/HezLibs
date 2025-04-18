import type { BaseLayoutProps } from 'fumadocs-ui/layouts/shared';
import { Avatar, AvatarImage } from '@/components/ui/avatar';
import { Group, PackageOpen, Signal } from 'lucide-react';

export const logo = (
  <>
    <Avatar className="md:w-8 md:h-8 w-6 h-6">
      <AvatarImage className="md:w-8 md:h-8 w-6 h-6" src="https://github.com/hezaerd.png" />
    </Avatar>
  </>
);

export const baseOptions: BaseLayoutProps = {
  githubUrl: 'https://github.com/hezaerd/hezlibs',
  nav: {
    title: (
      <>
        {logo}
        HezLibs
      </>
    ),
  },
  links: [
    {
      type: 'menu',
      text: 'Documentation',
      url: '/docs',
      items: [
        {
          icon: <Group />,
          text: 'FSM',
          description: 'Powerful finite state machine implementation',
          url: '/docs/fsm',
          menu: {
            className: 'lg:col-start-1',
          },
        },
        {
          icon: <Signal />,
          text: 'Signals',
          description: 'Signals',
          url: '/docs/signals',
          menu: {
            className: 'lg:col-start-1',
          },
        },
        {
          icon: <PackageOpen />,
          text: 'Monorepo Tools',
          description: 'Collection of tools to manage your Unity package monorepo',
          url: '/docs/monorepotools',
          menu: {
            className: 'lg:col-start-2 lg:row-start-1',
          },
        }
      ],
    },
  ],
};
