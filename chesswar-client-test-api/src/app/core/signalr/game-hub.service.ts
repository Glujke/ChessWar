import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class GameHubService {
  private connection: HubConnection | null = null;
  readonly isConnected = signal(false);
  readonly lastEvent = signal<{ name: string; payload: unknown } | null>(null);

  async connect(sessionId: string): Promise<void> {
    if (this.connection) return;
    this.connection = new HubConnectionBuilder()
      .withUrl(`/gameHub?sessionId=${encodeURIComponent(sessionId)}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.onreconnected(() => this.isConnected.set(true));
    this.connection.onclose(() => this.isConnected.set(false));

    // Универсальный слушатель для отладки и универсальных событий
    (this.connection as any).onAny?.((name: string, payload: unknown) => {
      this.lastEvent.set({ name, payload });
    });

    await this.connection.start();
    this.isConnected.set(true);
  }

  async joinGame(gameId: string): Promise<void> {
    await this.connection?.invoke('JoinGame', gameId);
  }

  on(eventName: string, handler: (payload: any) => void): void {
    this.connection?.on(eventName, handler);
  }

  off(eventName: string, handler: (payload: any) => void): void {
    this.connection?.off(eventName, handler);
  }
}


