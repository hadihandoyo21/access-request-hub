export interface AccessRequest {
  id: number;
  clientRequestId: string;
  requester: string;
  application: string;
  environment: string;
  accessLevel: string;
  justification: string;
  status: string;
  policyVersion: string;
  version: string;
  createdAt: string;
}

export interface AuditLog {
  id: number;
  requestId: number;
  actionType: string;
  actorEmail: string;
  reason?: string;
  timestamp: string;
}

export interface CreateRequestDto {
  clientRequestId: string;
  application: string;
  environment: string;
  accessLevel: string;
  justification: string;
}

export interface ApproveRequestDto {
  version: string;
}

export interface RejectRequestDto {
  version: string;
  reason: string;
}