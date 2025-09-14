import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { ServerSideRequest, ServerSideResponse, Bond, SubscriptionFilter } from '../types/Bond';

class SignalRService {
  private connection: HubConnection | null = null;
  private isConnected = false;
  private connectionPromise: Promise<void> | null = null;

  async connect(): Promise<void> {
    if (this.isConnected) return;
    
    // Return existing connection attempt if in progress
    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    this.connectionPromise = this.establishConnection();
    return this.connectionPromise;
  }

  private async establishConnection(): Promise<void> {
    console.log('Creating SignalR connection to http://localhost:5000/bondhub');
    
    this.connection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/bondhub')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.isConnected = true;
    });

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting due to error:', error);
      this.isConnected = false;
    });

    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.isConnected = false;
      this.connectionPromise = null;
    });

    try {
      await this.connection.start();
      this.isConnected = true;
      this.connectionPromise = null;
      console.log('SignalR connected with connection ID:', this.connection.connectionId);
    } catch (error) {
      this.connectionPromise = null;
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.isConnected = false;
      console.log('SignalR disconnected');
    }
  }

  async getBondRows(request: ServerSideRequest): Promise<ServerSideResponse> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection not established');
    }
    return await this.connection.invoke('GetBondRows', request);
  }

  async getTiersForBond(instrumentId: string): Promise<Bond[]> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection not established');
    }
    return await this.connection.invoke('GetTiersForBond', instrumentId);
  }

  async getDistinctValues(columnName: string): Promise<string[]> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection not established');
    }
    return await this.connection.invoke('GetDistinctValues', columnName);
  }

  async subscribeToFilter(filter: SubscriptionFilter): Promise<void> {
    if (!this.connection || !this.isConnected) {
      console.warn('SignalR not connected, skipping filter subscription');
      return;
    }
    await this.connection.invoke('SubscribeToFilter', filter);
  }

  onUpdateBond(callback: (bond: Bond) => void): void {
    if (!this.connection) throw new Error('Not connected');
    this.connection.on('UpdateBond', callback);
  }

  onBatchUpdateBonds(callback: (bonds: Bond[]) => void): void {
    if (!this.connection) throw new Error('Not connected');
    this.connection.on('BatchUpdateBonds', callback);
  }

  onRefreshGroup(callback: (path: string[]) => void): void {
    if (!this.connection) throw new Error('Not connected');
    this.connection.on('RefreshGroup', callback);
  }

  offUpdateBond(callback: (bond: Bond) => void): void {
    if (this.connection) {
      this.connection.off('UpdateBond', callback);
    }
  }

  offBatchUpdateBonds(callback: (bonds: Bond[]) => void): void {
    if (this.connection) {
      this.connection.off('BatchUpdateBonds', callback);
    }
  }

  offRefreshGroup(callback: (path: string[]) => void): void {
    if (this.connection) {
      this.connection.off('RefreshGroup', callback);
    }
  }
}

export const signalRService = new SignalRService();