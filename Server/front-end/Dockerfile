#Download project
FROM alpine/git as download
WORKDIR /download
RUN git clone https://github.com/jakobhviid/ScalableTeachingInSTEM-VM-Deployment scalable

#Final stage
FROM node:14 as base
WORKDIR /usr/src/app

RUN git clone https://github.com/jakobhviid/ScalableTeachingInSTEM-VM-Deployment
RUN npm install --production
EXPOSE 80

CMD ["node", "main.js"]