FROM node:20-alpine
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ENV CHOKIDAR_USEPOLLING=true
EXPOSE 4200

# важно: пробросить хост и порт
CMD ["npm","start","--","--host","0.0.0.0","--port","4200"]