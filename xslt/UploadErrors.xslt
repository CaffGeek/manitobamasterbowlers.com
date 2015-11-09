<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="/">
      <h2>Upload Failed...</h2>
      <ul>
        <xsl:for-each select="//error">
          <li>
            <xsl:value-of select="."/>
          </li>
        </xsl:for-each>
      </ul>
      Please correct the file and try uploading again.
    </xsl:template>
</xsl:stylesheet>
