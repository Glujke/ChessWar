FROM node:20-alpine

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Expose port
EXPOSE 4199

# Start development server with Docker proxy config
CMD ["npm", "run", "start", "--", "--host", "0.0.0.0", "--proxy-config", "proxy.conf.docker.json"]

