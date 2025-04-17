import type { BaseLayoutProps } from 'fumadocs-ui/layouts/shared';
import { Avatar, AvatarImage } from '@/components/ui/avatar';
import {
    Group,
    PackageOpen,
  } from 'lucide-react';

/**
 * Shared layout configurations
 *
 * you can customise layouts individually from:
 * Home Layout: app/(home)/layout.tsx
 * Docs Layout: app/docs/layout.tsx
 */
export const baseOptions: BaseLayoutProps = {
  githubUrl: 'https://github.com/hezaerd/hezlibs',
  nav: {
    title: (
      <>
        <Avatar className="w-8 h-8">
          <AvatarImage className="w-8 h-8" src="https://github.com/hezaerd.png" />
        </Avatar>
        HezLibs
      </>
    ),
  },
  links: [
    {
      type: 'menu',
      text: 'Documentation',
      url: '/docs/fsm',
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
          icon: <PackageOpen />,
          text: 'Monorepo Tools',
          description: 'Manage your Unity packages monorepo',
          url: '/docs/monorepotools',
          menu: {
            className: 'lg:col-start-2',
          },
        }
      ],
    },
  ],
};
