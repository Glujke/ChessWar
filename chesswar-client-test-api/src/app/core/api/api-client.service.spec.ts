import { of } from 'rxjs';
import { ApiClientService } from './api-client.service';
import { TutorialSessionDto } from './dtos';

describe('ApiClientService.startTutorial (TDD)', () => {
  it('POST /api/v1/game/tutorial?embed=(game) and maps ids', async () => {
    const post = jest.fn().mockReturnValue(
      of({ id: 't-42', _embedded: { game: { id: 'g-100' } } } as TutorialSessionDto)
    );
    const http = { post } as any;

    const api = new ApiClientService(http as any);
    const result = await api.startTutorial();

    expect(post).toHaveBeenCalledWith(
      '/api/v1/game/tutorial?embed=(game)',
      { playerId: 'web-client', showHints: true }
    );
    expect(result).toEqual({ tutorialId: 't-42', gameId: 'g-100' });
  });
});


