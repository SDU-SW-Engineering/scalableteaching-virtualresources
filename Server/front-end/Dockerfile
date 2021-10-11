#Download project
FROM alpine/git as download
WORKDIR /download
RUN git clone https://github.com/jakobhviid/ScalableTeachingInSTEM-VM-Deployment scalable

#Build
FROM node:14-alpine as build
WORKDIR /usr/src/app
COPY --from=download /download/scalable/Server/front-end/package*.json ./
RUN npm install
COPY --from=download /download/scalable/Server/front-end/ ./
RUN ls 


#Run
FROM nginx:stable-alpine as prod
COPY --from=build /usr/src/app/dist /usr/share/nginx/html
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]