// Add selectNodes and selectSingleNode to non IE browsers
if (!window.ActiveXObject) {
	Element.prototype.selectNodes = function(sXPath) {
		var oEvaluator = new XPathEvaluator();
		var oResult = oEvaluator.evaluate(sXPath, this, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);
		var aNodes = new Array();
		if (oResult != null) {
			var oElement = oResult.iterateNext();
			while (oElement) {
				aNodes.push(oElement);
				oElement = oResult.iterateNext();
			}
		}
		return aNodes;
	}

	Element.prototype.selectSingleNode = function(sXPath) {
		var oEvaluator = new XPathEvaluator();
		// FIRST_ORDERED_NODE_TYPE returns the first match to the xpath.
		var oResult = oEvaluator.evaluate(sXPath, this, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
		if (oResult != null) {
			return oResult.singleNodeValue;
		} else {
			return null;
		}
	}
}

// Add innerXml to non IE browsers
if (!window.ActiveXObject) {
    Element.prototype.innerXml = function() {
	    return innerXml(this);
    }
}

//HACK: Create innerXml function for IE...FUCK!
function innerXml(element) {
    var result = '';
    var children = element.childNodes;
	
    var l = children.length;
    for (var i = 0; i < l; i++) {
        result += children[i].xml || (new XMLSerializer()).serializeToString(children[i]);
    }
	
    return result;
}