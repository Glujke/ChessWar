import { TestBed } from '@angular/core/testing';
import { ResultOverlayComponent } from './result-overlay.component';

describe('ResultOverlayComponent', () => {
  it('emits events and shows win text', () => {
    TestBed.configureTestingModule({ imports: [ResultOverlayComponent] });
    const fixture = TestBed.createComponent(ResultOverlayComponent);
    const cmp = fixture.componentInstance;
    cmp.visible = true;
    cmp.isWin = true;
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Победа!');

    const toMenuSpy = jest.fn();
    const replaySpy = jest.fn();
    cmp.toMenu.subscribe(toMenuSpy);
    cmp.replay.subscribe(replaySpy);

    const buttons = fixture.nativeElement.querySelectorAll('button');
    (buttons[0] as HTMLButtonElement).click();
    (buttons[1] as HTMLButtonElement).click();

    expect(toMenuSpy).toHaveBeenCalled();
    expect(replaySpy).toHaveBeenCalled();
  });
});


