import { TestBed } from '@angular/core/testing';
import { PieceContextComponent } from './piece-context.component';
import { signal } from '@angular/core';
import { GameViewModel } from './game.view-model';

describe('PieceContextComponent', () => {
  it('shows selected piece info and abilities', () => {
    const vmStub = {
      selectedPiece: signal<any>({ id: 'X', type: 'Pawn', hp: 10, attack: 2, movement: 2, range: 1 }),
      getAbilitiesForSelected: () => [{ name: 'ShieldBash', manaCost: 2, cooldown: 0 }],
      showAbilityTargets: jest.fn()
    } as Partial<GameViewModel> as GameViewModel;

    TestBed.configureTestingModule({
      imports: [PieceContextComponent],
      providers: [{ provide: GameViewModel, useValue: vmStub }]
    });

    const fixture = TestBed.createComponent(PieceContextComponent);
    fixture.componentInstance.gameId = 'g1';
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Фигура');
    expect(text).toContain('Тип: Pawn');
    expect(text).toContain('HP: 10');
    expect(text).toContain('ATK: 2');
    expect(text).toContain('Move: 2');
    expect(text).toContain('Range: 1');
    expect(text).toContain('ShieldBash');
  });
});


