export enum WidgetId {
  KpiSummary = 'kpi_summary',
  Insights = 'insights',
  Tickets = 'tickets',
  ThreeDModel = 'three_d_model',
  Location = 'location',
}

export const isWidgetId = (id: string): id is WidgetId =>
  Object.values(WidgetId).includes(id as WidgetId)
