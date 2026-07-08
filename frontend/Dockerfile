# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM node:20-alpine AS build
WORKDIR /app

# Copy lockfile + package.json first so `npm ci` is cached in its own layer.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite bakes VITE_* env vars into the built JS bundle at BUILD time — this runs in the user's
# browser, not container-to-container, so it must be a URL the browser (not the Docker network)
# can reach. Default matches the API container's published host port (see docker-compose.yml).
ARG VITE_API_BASE_URL=http://localhost:5019/api/v1
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# ---- Final (static-serve) stage ----
FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# nginx:alpine ships busybox wget, used here instead of curl (not present in this image).
HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=5 \
    CMD wget --quiet --tries=1 --spider http://localhost/health || exit 1
