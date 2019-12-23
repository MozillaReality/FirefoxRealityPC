"use strict";

const CUSTOM_USER_AGENT = 'Mozilla/5.0 (Linux; Android 7.1.1; Quest) AppleWebKit/537.36 (KHTML, like Gecko) OculusBrowser/7.0.13.186866463 SamsungBrowser/4.0 Chrome/77.0.3865.126 Mobile VR Safari/537.36';

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

browser.webRequest.onBeforeSendHeaders.addListener(rewriteUserAgentHeader,
  {urls: targets},
  ["blocking", "requestHeaders"]);
