import { Badge } from '@mantine/core';
import { RISCO_COR, RISCO_LABEL, STATUS_COR, STATUS_LABEL } from '../domain/labels';
import type { RiscoProjeto, StatusProjeto } from '../types';

export function StatusBadge({ status }: { status: StatusProjeto }) {
  return (
    <Badge color={STATUS_COR[status]} variant="light">
      {STATUS_LABEL[status]}
    </Badge>
  );
}

export function RiscoBadge({ risco }: { risco: RiscoProjeto }) {
  return (
    <Badge color={RISCO_COR[risco]} variant="dot">
      {RISCO_LABEL[risco]}
    </Badge>
  );
}
