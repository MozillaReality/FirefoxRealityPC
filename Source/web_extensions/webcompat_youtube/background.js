"use strict";

const CUSTOM_USER_AGENT = 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12) AppleWebKit/602.1.21 (KHTML, like Gecko) Version/9.2 Safari/602.1.21';

const targets = [
        "*://*.youtube.com/*",
        "*://*.youtube-nocookie.com/*"
      ];

function rewriteUserAgentHeader(e) {
  for (var header of e.requestHeaders) {
    if (header.name.toLowerCase() === "user-agent") {
      header.value = CUSTOM_USER_AGENT;
    }
  }
  return {requestHeaders: e.requestHeaders};
}

/**
 * 1. Disable YouTube's Polymer layout (which makes YouTube very slow in non-Chrome browsers)
 *    via a query-string parameter in the URL.
 * 2. Rewrite YouTube URLs from `m.youtube.com` -> `youtube.com` (to avoid serving YouTube's
 *    video pages intended for mobile phones, as linked from Google search results).
 */
function redirectUrl(req) {
  let redirect = false;
  const url = new URL(req.url);
  if (url.host.startsWith("m.")) {
    url.host = url.host.replace("m.", "www.");
    redirect = true;
  }
  if (!url.searchParams.get("disable_polymer")) {
    url.searchParams.set("disable_polymer", "1");
    redirect = true;
  }
  if (!redirect) {
    return null;
  }
  return { redirectUrl: url.toString() };
}

browser.webRequest.onBeforeSendHeaders.addListener(rewriteUserAgentHeader,
  {urls: targets},
  ["blocking", "requestHeaders"]);

browser.webRequest.onBeforeRequest.addListener(redirectUrl,
  {urls: targets, types: ["main_frame"]},
  ["blocking"]);
