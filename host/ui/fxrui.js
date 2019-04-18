 // fxrui.js

// Configuration vars
let homeURL = "https://mixedreality.mozilla.org/firefox-reality/"


// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XUL/browser
let browser = null;
let urlInput = null;

window.addEventListener("DOMContentLoaded", () => {
  urlInput = document.getElementById("eUrlInput");
  
  setupBrowser();
  setupNavButtons();
  setupUrlBar();
  setupMenu();
  
}, { once: true });


// Create XUL browser object
function setupBrowser() {
  browser = document.createXULElement("browser");
  browser.setAttribute("type", "content");
  browser.homePage = homeURL;
  document.querySelector("#browser-container").append(browser);
  
  urlInput.value = homeURL;
  browser.setAttribute("src", homeURL);
  
  /*
  TODO: figure out how to get URL to update with each navigation
  browser.addProgressListener({
    QueryInterface: ChromeUtils.generateQI([Ci.nsIWebProgressListener, Ci.nsISupportsWeakReference]),
    onLocationChange(aWebProgress, aRequest, aLocation, aFlags) {
      console.log(browser.currentURI.spec);
      urlInput.value = browser.currentURI.spec;
    },
  });
  */
 
}

// Setup common behavior for all of the navigation buttons
function setupNavButtons() {
  let aryNavButtons = ["eBack", "eForward", "eRefresh", "eHome"];
  let aryNavFunction =["goBack", "goForward", "reload", "goHome"];

  aryNavButtons.forEach(function(btnName, index) {
    let elem = document.getElementById(btnName);
    
    // On click, do the action
    elem.addEventListener("click", function() {
      browser[aryNavFunction[index]]();
    });
    
    // Events that update UI visuals
    elem.addEventListener("mousedown", function() {
      elem.classList.replace("nav-button", "nav-button-click");
      elem.classList.replace("nav-button-icon", "nav-button-icon-click");
    });
    
    elem.addEventListener("mouseup", function() {
      elem.classList.replace("nav-button-click", "nav-button");
      elem.classList.replace("nav-button-icon-click", "nav-button-icon");
    });
  });
}

function setupUrlBar() {  
  let container = document.getElementById("eUrlBarContainer");
  let clearButton = document.getElementById("eUrlClear");
  
  // Navigate to new value after the element changes
  urlInput.addEventListener("change", function() {
    browser.setAttribute("src", urlInput.value);
  });
  
  // Upon focus, highlight the whole URL
  urlInput.addEventListener("focus", function() {
    container.classList.replace("nav-urlbar", "nav-urlbar-click");
    urlInput.select();
    clearButton.classList.remove("hide-elem");
    
  });
  
  urlInput.addEventListener("blur", function() {
    container.classList.replace("nav-urlbar-click", "nav-urlbar");
    clearButton.classList.add("hide-elem");
  });
  
  clearButton.addEventListener("click", function() {
    urlInput.value = "";
  });  
}

function setupMenu() {
  let container = document.getElementById("eMenuContainer");
  let icon = document.getElementById("eMenuIcon");
  container.addEventListener("mouseenter", function() {
    icon.classList.replace("nav-button-icon", "nav-menu-button-hover");
  });
  
  container.addEventListener("mouseleave", function() {
    icon.classList.replace("nav-menu-button-hover", "nav-button-icon");
  });
}