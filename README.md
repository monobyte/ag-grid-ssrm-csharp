# Bond Trading Dashboard

A real-time bond trading dashboard with AG-Grid Server-Side Row Model, custom grouping, and tier expansion. Built with .NET Core 8 backend and React frontend using SignalR for real-time updates.

## Architecture Overview

- **Backend**: .NET Core 8 Web API with SignalR Hub
- **Frontend**: React + TypeScript + Vite + AG-Grid
- **Communication**: SignalR for real-time data and server-side row model requests
- **Data**: 3000 mock bond records with 5 pricing tiers each
- **Real-time**: Simulated price updates every 250ms

## Features

### Backend
- Mock data generation for 3000 bonds with 5 tiers each
- SignalR BondHub with server-side row model endpoints
- Real-time tick simulation with configurable update rates
- Server-side filtering, sorting, grouping, and pagination
- Subscription management for targeted updates

### Frontend
- AG-Grid with Server-Side Row Model and tree data structure
- Custom grouping UI (not built-in AG-Grid grouping)
- Bond tier expansion (Tier1 as parent, Tier2-5 as children)
- Real-time updates via SignalR
- Filtering by currency and sector
- Responsive design with Alpine theme

## Running the Application

### Backend (.NET Core 8)
```bash
cd backend
dotnet run
```
The API will start on `http://localhost:5000` with SignalR hub at `/bondhub`.

### Frontend (React + Vite)
```bash
cd frontend
pnpm install
pnpm dev
```
The frontend will start on `http://localhost:5173`.

## Configuration

### Backend Configuration (appsettings.json)
- `BondCount`: Number of bonds to generate (default: 3000)
- `TickIntervalMs`: Tick simulation interval (default: 250ms)
- `UpdatePercentage`: Percentage of bonds to update per tick (default: 2%)

### Frontend Configuration
- Backend URL is configured in `SignalRService.ts`
- AG-Grid settings: `cacheBlockSize: 100`, `maxBlocksInCache: 10`

## Data Model

### Bond Fields (20 total)
- `instrumentId`: Unique identifier (e.g., "BOND0001")
- `name`, `issuer`, `currency`, `sector`
- `maturityDate`, `couponRate`, `faceValue`
- `bid`, `ask`, `spread` (calculated), `yield`
- `openingPrice`, `closingPrice`, `lastPrice`, `volume`
- `updateTime`, `rating`, `isin`, `cusip`, `tierId`

### Data Distribution
- **Currencies**: EUR (40%), GBP (30%), USD (30%)
- **Sectors**: Government (50%), Corporate (30%), Municipal (20%)
- **Tiers**: Tier1 (default/parent), Tier2-5 (children with tighter spreads)

## Usage

1. **Filtering**: Use the filter panel to subscribe to specific currencies or sectors
2. **Grouping**: Configure custom grouping by selecting and ordering columns
3. **Tier Expansion**: Click bond rows (with group icons) to expand and view tier pricing
4. **Real-time Updates**: Prices update automatically via SignalR subscriptions

## Architecture Notes

- No authentication or persistence (in-memory data)
- Server-side logic handles all filtering, sorting, and grouping
- Tree data structure used instead of Master/Detail for tier display
- SignalR subscriptions are shared across users and filtered server-side
- Custom grouping UI instead of AG-Grid's built-in drag-to-group

## macOS Compatibility

Both projects are configured for macOS compatibility with no platform-specific dependencies.