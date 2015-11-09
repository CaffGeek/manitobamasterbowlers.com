<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="//Announcement">
    <tr class="Announcement">
      <td>
        <span class="Announcement">
          <xsl:copy-of select="Value/node()"/>
        </span>
        <xsl:text>Start: </xsl:text>
        <span class="StartDate">
          <xsl:value-of select="@StartDate"/>
        </span>
        <xsl:text xml:space="preserve">  </xsl:text>
        <xsl:text>Stop: </xsl:text>
        <span class="EndDate">
          <xsl:value-of select="@EndDate"/>
        </span>
      </td>
      <td>
        <div class="StyledButton Blue" onclick="javascript:Edit_Click(this)">Edit</div>
        <div class="StyledButton Red" onclick="javascript:Delete_Click(this)">Delete</div>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>