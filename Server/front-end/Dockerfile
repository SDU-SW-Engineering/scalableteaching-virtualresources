#Build
FROM node:16 as build
WORKDIR /src
COPY ./Server/front-end/package*.json ./
RUN npm install
COPY ./Server/front-end/ ./
RUN npm run build

#Run
FROM nginx:stable-alpine as prod
COPY --from=build /src/dist /app
EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
