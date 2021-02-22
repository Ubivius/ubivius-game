# BUILD_TYPE can have these values: local, test or prod
# if BUILD_TYPE is empty, set to local
ARG BUILD_TYPE=local
FROM gcr.io/distroless/base as prod

FROM alpine as test

FROM ubuntu as local

FROM ${BUILD_TYPE} AS exit_artefact
COPY ./build/Server ./app
CMD ["app/ubivius-server.x86_64"]
