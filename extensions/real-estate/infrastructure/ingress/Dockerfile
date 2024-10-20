# Dockerfile used for hosting of Ephemeral development environments
# https://github.com/WillowInc/rfcs/discussions/59
FROM nginx:1.21-alpine

RUN chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d && \
    chown -R nginx:nginx /usr/share/nginx/html
RUN touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

ENV BUILD_DEPS="gettext"  \
    RUNTIME_DEPS="libintl"

RUN set -x && \
    apk add --update $RUNTIME_DEPS && \
    apk add --virtual build_deps $BUILD_DEPS &&  \
    cp /usr/bin/envsubst /usr/local/bin/envsubst && \
    apk del build_deps

COPY nginx.conf /opt/nginx.conf.env

USER nginx:nginx
EXPOSE 8080
CMD ["/bin/sh", "-c", "envsubst '$IMAGEHUB_URL,$PORTALXL_URL,$PORTALWEB_URL' < /opt/nginx.conf.env > /var/cache/nginx/nginx.conf && nginx -g \"daemon off;\" -c /var/cache/nginx/nginx.conf"]