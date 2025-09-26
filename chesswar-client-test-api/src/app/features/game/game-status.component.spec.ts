import { TestBed } from '@angular/core/testing';
import { GameStatusComponent } from './game-status.component';
import { signal } from '@angular/core';
import { GameViewModel } from './game.view-model';

describe('GameStatusComponent', () => {
  it('renders turn number, side to move and mana', () => {
    const vmStub = {
      session: signal<any>({ currentTurn: { number: 5 } }),
      isMyTurn: () => true,
      manaText: () => '7/10'
    } as Partial<GameViewModel> as GameViewModel;

    TestBed.configureTestingModule({
      imports: [GameStatusComponent],
      providers: [{ provide: GameViewModel, useValue: vmStub }]
    });

    const fixture = TestBed.createComponent(GameStatusComponent);
    fixture.componentInstance.gameId = 'g1';
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Ход: 5');
    expect(text).toContain('Чей ход: Player');
    expect(text).toContain('Мана: 7/10');
  });
});


