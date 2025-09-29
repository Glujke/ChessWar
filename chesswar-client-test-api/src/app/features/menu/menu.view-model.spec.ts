import { signal } from '@angular/core';
import { MenuViewModel } from './menu.view-model';

interface StartTutorialResponse {
  tutorialId: string;
  gameId: string;
}

interface ApiClientLike {
  startTutorial: () => Promise<StartTutorialResponse>;
}

interface RouterLike {
  navigate: (commands: unknown[]) => Promise<boolean> | void;
}

const createVm = (deps: { api?: ApiClientLike; router?: RouterLike } = {}) => {
  const api = deps.api ?? { startTutorial: async () => ({ tutorialId: 't1', gameId: 'g1' }) };
  const router = deps.router ?? { navigate: () => {} };
  return new MenuViewModel(api as any, router as any);
};

describe('MenuViewModel (TDD)', () => {
  it('tutorial button is enabled by default', () => {
    const vm = createVm();
    expect(vm.isTutorialEnabled()).toBe(true);
  });

  it('click tutorial calls API then navigates to /game/:id', async () => {
    const startTutorial = jest.fn().mockResolvedValue({ tutorialId: 't-123', gameId: 'g-777' });
    const navigate = jest.fn();
    const vm = createVm({ api: { startTutorial }, router: { navigate } });

    await vm.onClickTutorial();

    expect(startTutorial).toHaveBeenCalledTimes(1);
    expect(navigate).toHaveBeenCalledWith(['/gamesession', 'g-777']);
  });
});


