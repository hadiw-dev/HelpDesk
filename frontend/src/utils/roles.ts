const AGENT_OR_ABOVE = ['Admin', 'Manager', 'IT Support Agent']
const MANAGER_OR_ADMIN = ['Admin', 'Manager']

export const isAgentOrAbove = (roles: string[]) => roles.some((role) => AGENT_OR_ABOVE.includes(role))
export const isManagerOrAdmin = (roles: string[]) => roles.some((role) => MANAGER_OR_ADMIN.includes(role))
