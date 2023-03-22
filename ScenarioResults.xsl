<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="2.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:user="urn:my-scripts">

  <!--xmlns:fo="http://www.w3.org/1999/XSL/Format"-->

  <xsl:output method="html" indent="yes"/>

  <!--<msxsl:script language="javascript">
    </msxsl:script>
   
  <style type="text/css">
    .row { vertical-align: top; height:auto !important; }
    .list {display:none; }
    .show {display: none; }
    .hide:target + .show {display: inline; }
    .hide:target {display: none; }
    .hide:target ~ .list {display:inline; }
    @media print { .hide, .show { display: none; } }
  </style>-->

  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[
    public string GetShortDateTime(string time)
    {
    
        return DateTime.Parse(time).ToString();
    }
    
    public string CurrentDateTime()
    {
        return DateTime.Now.ToString();
    }
    
    public string GetShortTimeSpanDifference(string end, string start)
    {
        DateTime t0 = DateTime.Parse(start);
        DateTime t1 = DateTime.Parse(end);
        
        TimeSpan ts = t1 - t0;
        
        return ts.ToString(@"hh\:mm\:ss");
    }
    
    public string ReplaceErrorLabel(string cellValue)
    {
       return cellValue.Replace("##ERROR", "").Trim();      
    }
 
  
    ]]>
  </msxsl:script>

  <xsl:template match="Results">
    <html>

      <head>

        <!--<style type="text/css">
          .row { vertical-align: top; height:auto !important; }
          .list {display:none; }
          .show {display: none; }
          .hide:target + .show {display: inline; }
          .hide:target {display: none; }
          .hide:target ~ .list {display:inline; }
          @media print { .hide, .show { display: none; } }
        </style>-->

        <title>Test Results</title>
        <style>
          body {font-family: Verdana, "Courier New", Courier, monospace;}
          h1 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; width: 100%; color: #ffffff; text-align: center; background-color: #922a72; border-style: solid; border-color: #74215b; border-bottom-width: 8px; border-top-width: 8px; border-left-width: 0px; border-right-width: 0px;}
          h2 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; clear: both; color: #74215b;}
          h3 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; clear: both; color: #74215b;}
          h4 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; clear: both; color: #74215b;}
          h5 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; clear: both; color: #74215b;}
          h6 {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; clear: both; color: #74215b;}
          pre {padding-left: 8px; padding-right: 8px;}
          a.top {padding-left: 8px; padding-right: 8px;}
          table.fixed {table-layout: fixed;border-collapse: collapse; }
          table, th, td {border: 1px solid #922a72; border-collapse: collapse; font-size:9pt;}

          .noborder{
          border:0px;
          }

          legend {
          border: 0px solid #ffffff;
          border-style: solid;
          border-color: #74215b;
          border-bottom-width: 8px;
          border-top-width: 8px;
          border-left-width: 20px;
          border-right-width: 0px;
          padding-left: 8px;
          font-size:7pt;
          table-layout: fixed;border-collapse: collapse;
          }

          td.Indent{
          text-indent: 50px;
          }

          .smallFont{font-size:7pt}
          .mediumFont{font-size:8pt}

          th {color: #74215b;}
          td {padding-top: 4px; padding-bottom: 4px; padding-left: 8px; padding-right: 8px; word-wrap:break-word; vertical-align: top;}
          td.highlight {background-color: #ce55aa; color: #74215b;}

          <!--https://www.w3schools.com/colors/colors_picker.asp-->

          tr.stepName {background-color: #b3e6ff; color: #74215b;}
          tr.stepDescription {background-color: #ffffb3; color: #74215b;}
          <!--tr.stepEntry {background-color: #99ffcc; color: #74215b;}-->
          tr.stepError {background-color: #ff1a1a; color: #74215b;}

          tr.stepEntry:nth-child(odd) { background-color: #99ffcc; color: #74215b;}
          tr.stepEntry:nth-child(even) { background-color: #99daff; color: #74215b;}


          .stepName {background-color: #b3e6ff; color: #74215b;}
          .stepDescription {background-color: #ffffb3; color: #74215b;}
          .stepEntry {background-color: #99ffcc; color: #74215b;}
          .stepError {background-color: #ff1a1a; color: #74215b;}


          .Pass {background-color: #99ffcc; color: #74215b;}
          .Fail {background-color: #ff1a1a; color: #74215b;}
          .Warning {background-color: #ffa64d; color: #74215b;}

          .bold {font-weight: bold}

          <!--td[value="Pass"]{
            color: green;
          }
          
          td[status_result=Pass]:after{
            content attr(status_result)
            color: green
          }
          
          td[status_result=Fail]:after{
          content attr(status_result)
          color: green
          }
          td[status_result=Warning]:after{
          content attr(status_result)
          color: orange
          }-->

          <!--.accordion {
          background-color: #eee;
          color: #444;
          cursor: pointer;
          padding: 18px;
          width: 100%;
          border: none;
          text-align: left;
          outline: none;
          font-size: 15px;
          transition: 0.4s;
          }

          .active, .accordion:hover {
          background-color: #ccc;
          }

          .accordion:after {
          content: '\002B';
          color: #777;
          font-weight: bold;
          float: right;
          margin-left: 5px;
          }

          .active:after {
          content: "\2212";
          }

          .panel {
          padding: 0 18px;
          background-color: white;
          max-height: 0;
          overflow: hidden;
          transition: max-height 0.2s ease-out;
          }-->

        </style>
      </head>

      <body>

        <h1>
          <a name="Top">Test Results</a>

        </h1>

        <h2>Test Results Summary</h2>
        <table cellpadding="0" cellspacing="0" border="0" class="noborder">
          <tr>
            <td class="noborder">
              <xsl:apply-templates select="BreakDown"/>
            </td>
          </tr>
        </table>
        <br />

        <h3>
          Start the Script run for: <xsl:value-of select="//StartTime" />
        </h3>
        <h3>Test Steps</h3>


        <table class="legend">
          <tr class="noborder mediumFont">&#160; &#160; Legend</tr>

          <tr border="1" border-style="solid">
            <td class="noborder">    </td>
            <td class="stepName noborder smallFont"></td>
            <td class="noborder smallFont">Step Name</td>
            <td class="noborder">    </td>
            <td class="stepDescription  noborder smallFont"></td>
            <td class="noborder smallFont">Step Description</td>
            <td class="noborder">  </td>
            <td class="stepEntry noborder smallFont"> </td>
            <td class="noborder smallFont">Step Entry</td>
          </tr>

        </table>

        <table cellpadding="0" cellspacing="0" border="0" class="noborder">
          <tr>
            <td class="noborder">
              <xsl:apply-templates select="TestSteps"/>
            </td>
          </tr>
        </table>

      </body>

    </html>

  </xsl:template>


  <xsl:template match="BreakDown">
    <table cellpadding="0" cellspacing="0" border="0" class="summary">
      <tr>
        <td class="highlight">Feature</td>
        <td>
          <xsl:value-of select="//Feature" />
        </td>
      </tr>
      <tr>
        <td class="highlight">Scenario</td>
        <td>
          <xsl:value-of select="//Scenario" />
        </td>
      </tr>
      <tr>
        <td class="highlight">Start Time</td>
        <td>
          <xsl:value-of select="//StartTime" />
        </td>
      </tr>
      <tr>
        <td class="highlight">End Time</td>
        <td>
          <xsl:value-of select="//EndTime" />
        </td>
      </tr>

      <tr>
        <td class="highlight">Elapsed</td>
        <td>
          <xsl:value-of select="//ElapsedTime" />
        </td>
      </tr>

      <tr>
        <td class="highlight">RunBy</td>
        <td>
          <xsl:value-of select="//RunBy" />
        </td>
      </tr>

      <tr>
        <td class="highlight">Machine</td>
        <td>
          <xsl:value-of select="//Machine" />
        </td>
      </tr>

      <tr>
        <td class="highlight">Result</td>

        <xsl:variable name="STATUS">
          <xsl:value-of select="//Result"/>
        </xsl:variable>

        <td class="{$STATUS}" >

          <xsl:value-of select ="$STATUS" />
        </td>

      </tr>
    </table>


  </xsl:template>

  <xsl:template match="TestSteps">

    <table class="noborder" cellpadding="0" cellspacing="0" border="0" width="100%" style="border-collapse:collapse">
      <tbody>
        <tr class="stepName">
          <th width="190"></th>
          <th></th>
          <th></th>
          <th></th>
        </tr>
        <xsl:apply-templates select="Step" />
      </tbody>
    </table>

  </xsl:template>

  <xsl:template match="Step">

    <tr class="stepName">

      <!--<xsl:variable name="stepStatus"><xsl:value-of select="@result"/></xsl:variable>-->

      <xsl:choose>
        <xsl:when test="@result='Fail'">

          <td class="bold">
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="bold Fail">Fail</td>
          <td class="bold">
            <xsl:value-of select="@name" />
          </td>

        </xsl:when>

        <xsl:otherwise>

          <td >
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="{@result}" >
            <xsl:value-of select ="@result" />
          </td>
          <td>
            <xsl:value-of select="@name" />
          </td>

        </xsl:otherwise>
      </xsl:choose>

    </tr>

    <xsl:if test="//StepTable">
      <xsl:apply-templates select="StepTable" />
    </xsl:if>

    <xsl:apply-templates select="InnerStep" />

  </xsl:template>

  <xsl:template match="InnerStep">

    <tr class="stepDescription">

      <xsl:choose>

        <xsl:when test="@result='Fail'">

          <td class="bold ">
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="bold Fail" >Fail</td>
          <td class="bold ">
            <xsl:value-of select="@name" />
          </td>

        </xsl:when>
        <xsl:otherwise>

          <td>
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="{@result}" >
            <xsl:value-of select ="@result" />
          </td>
          <td>
            <xsl:value-of select="@name" />
          </td>

        </xsl:otherwise>
      </xsl:choose>

    </tr>

    <xsl:apply-templates select="LogEntry" >
      <xsl:with-param name="stepStatus" select="@result"/>
    </xsl:apply-templates>

  </xsl:template>

  <xsl:template match="LogEntry">
    <xsl:param name="stepStatus"/>

    <tr class="stepEntry">

      <xsl:if test="//headers">
        <xsl:apply-templates select="StepTables">

        </xsl:apply-templates>
      </xsl:if>

      <xsl:choose  >

        <xsl:when test="$stepStatus='Fail' and count(@Type) &lt; 1">

          <td class="bold">
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="bold Fail">Fail</td>
          <td class="bold">

            <xsl:value-of select="@comment"/>

            <xsl:if test="count(@expected) > 0">
              <table class="noborder">
                <tr class="noborder">
                  <td class="noborder bold">
                    <xsl:text>Expected: </xsl:text>
                  </td>
                  <td class="noborder">
                    <xsl:value-of select="@expected" />
                  </td>
                </tr>
                <tr class="noborder">
                  <td class="noborder bold">
                    <xsl:text>Actual: </xsl:text>
                  </td>
                  <td class="noborder">
                    <xsl:value-of select="@actual" />
                  </td>
                </tr>
              </table>
            </xsl:if>

            <!--<xsl:if test="@ScreenshotID = true() and @ScreenshotID !=''">
              -->
            <!--<tr>-->
            <!--
                <td>
                  <xsl:value-of select="@timestamp" />
                </td>
                <td class="Fail" >
                  <xsl:value-of select ="@result" />
                </td>
                <td>
                  <a href="@ScreenshotID">Click on Link</a>
                </td>
              -->
            <!--</tr>-->
            <!--
            </xsl:if>-->

          </td>
        </xsl:when>


        <xsl:when test="@result = true() and @result != ''">

          <td>
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="{@result}" >
            <xsl:value-of select ="@result" />
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@host = true() and @host != ''">
                <b>
                  Server Info: [ <xsl:value-of select="@host" /> ]
                </b>
                <br></br>
                <xsl:value-of select="@comment" />
              </xsl:when>

              <xsl:otherwise>
                <xsl:value-of select="@comment" />
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </xsl:when>

        <xsl:when test="@status = true() and @status != ''">

          <td>
            <xsl:value-of select="@timestamp" />
          </td>
          <td class="{@status}" >
            <xsl:value-of select ="@status" />
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@host = true() and @host != ''">
                <b>Server Info: [ <xsl:value-of select="@host" /> ]</b>
                <br></br>
                <xsl:value-of select="@comment" />
              </xsl:when>

              <xsl:otherwise>
                <xsl:value-of select="@comment" />
              </xsl:otherwise>
            </xsl:choose>
          </td>

        </xsl:when>

      </xsl:choose>

      <xsl:if test="@ScreenshotID = true() and @ScreenshotID !=''">

        <td>
          <xsl:value-of select="@timestamp"/>
        </td>
        <td class="{$stepStatus}">
          <xsl:value-of select ="$stepStatus"/>
        </td>
        <td>
          <a href="{@ScreenshotID}">Click on Screenshot Link</a>
        </td>

      </xsl:if>



    </tr>

  </xsl:template>

  <xsl:template match="StepTable">

    <tr bgcolor="#e0e0eb">
      <td class="noborder"></td>
      <td class="noborder" colspan="3" align="left" cellpadding="15">
        <table border="1" cellpadding="5" cellspacing="0" bgcolor="#efeff5"
               style="font-size:70%">
          <tbody>
            <tr>
              <th colspan="99">Table Data</th>
            </tr>

            <tr colspan="3">
              <headers>
                <xsl:for-each select="descendant::*[self::th]">
                  <th text-align="center">
                    <xsl:value-of select="."/>
                  </th>
                </xsl:for-each>

              </headers>

            </tr>
          </tbody>
          <xsl:apply-templates select="Rows"></xsl:apply-templates>
        </table>
      </td>

    </tr>


  </xsl:template>

  <xsl:template match="Rows">

    <xsl:for-each select="descendant::tr">
      <tr>
        <xsl:apply-templates select="descendant::td"></xsl:apply-templates>
      </tr>
    </xsl:for-each>

  </xsl:template>

  <xsl:template match ="td">


    <xsl:if test="contains(.,'##ERROR')">

      <xsl:variable name ="cellValue" select="."/>
      <td class="bold fail">
        <xsl:value-of select="user:ReplaceErrorLabel($cellValue)" />
      </td>

    </xsl:if>

    <xsl:if test="not(contains(.,'##ERROR'))">
      <td>
        <xsl:value-of select="."/>
      </td>
    </xsl:if>


  </xsl:template>


  <!--<xsl:template name="expandIt">
    <script language="JavaScript">
      function expandIt(whichEl, link) {
        whichEl.style.display = (whichEl.style.display == "none" ) ? "" : "none";
        if ( link ) { 
            if ( link.innerHTML ) {
              if ( whichEl.style.display == "none" ) {
                link.innerHTML = "[+] " + link.innerHTML.substring( 4 );
              } else {
                link.innerHTML = "[-] " + link.innerHTML.substring( 4 );
              }
            }
          }
        }
    </script>       
  </xsl:template>-->

</xsl:stylesheet>
