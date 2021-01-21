<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OnlineEditorsExample._Default" Title="ONLYOFFICE" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web.Configuration" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>ONLYOFFICE</title>

    <link rel="icon" href="~/favicon.ico" type="image/x-icon" />

    <link rel="stylesheet" type="text/css" href="https://fonts.googleapis.com/css?family=Open+Sans:900,800,700,600,500,400,300&subset=latin,cyrillic-ext,cyrillic,latin-ext" />

    <link rel="stylesheet" type="text/css" href="app_themes/stylesheet.css" />

    <link rel="stylesheet" type="text/css" href="app_themes/jquery-ui.css" />

    <script language="javascript" type="text/javascript" src="script/jquery-1.9.0.min.js"></script>

    <script language="javascript" type="text/javascript" src="script/jquery-ui.min.js"></script>

    <script language="javascript" type="text/javascript" src="script/jquery.blockUI.js"></script>

    <script language="javascript" type="text/javascript" src="script/jquery.iframe-transport.js"></script>

    <script language="javascript" type="text/javascript" src="script/jquery.fileupload.js"></script>

    <script language="javascript" type="text/javascript" src="script/jquery.dropdownToggle.js"></script>

    <script language="javascript" type="text/javascript" src="script/jscript.js"></script>

    <script language="javascript" type="text/javascript">
        var ConverExtList = '<%= string.Join(",", OnlineEditorsExample.DefaultConfigTools.ConvertExts.ToArray()) %>';
        var EditedExtList = '<%= string.Join(",", OnlineEditorsExample.DefaultConfigTools.EditedExts.ToArray()) %>';
    </script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="top-panel"></div>
        <div class="main-panel">
      
            <table class="user-block-table" cellspacing="0" cellpadding="0">
                <tbody>
                    <tr>
                        <td width="30%" valign="middle">
                            <span class="select-user">Username:</span>
                            <select class="select-user" id="user">
                                <option value="uid-1">John Smith</option>
                                <option value="uid-2">Mark Pottato</option>
                                <option value="uid-3">Hamish Mitchell</option>
                            </select>
                        </td>
                        <td width="70%" valign="middle"></td>
                    </tr>
                    <tr>
                        <td width="30%" valign="middle">
                            <select class="select-user" id="language">
                                <option value="en">English</option>
                                <option value="bg">Bulgarian</option>
                                <option value="zh">Chinese</option>
            
                            </select>
                        </td>
                        <td width="70%" valign="middle"></td>
                    </tr>
                </tbody>
            </table>
            <br />
      
<input id="File1" type="file"  />
            <div class="help-block">
                <span class="try-descr">Upload your file or create new file</span>
                <br />
                <br />
                <div class="clearFix">
                    <div class="upload-panel clearFix">
                        <a class="file-upload">
                            Upload
                            <br />
                            File
                            <input type="file" id="fileupload" name="files[]" data-url="webeditor.ashx?type=upload" />
                        </a>
                    </div>
                    <div class="create-panel">
                        <ul class="try-editor-list clearFix">
                            <li><a class="try-editor document" data-type="document">Create<br />Document</a></li>
                            <li><a class="try-editor spreadsheet" data-type="spreadsheet">Create<br />Spreadsheet</a></li>
                            <li><a class="try-editor presentation" data-type="presentation">Create<br />Presentation</a></li>
                        </ul>
                        <label class="create-sample">
                            <input id="createSample" class="checkbox" type="checkbox" />
                            Create a file filled with sample content
                        </label>
                    </div>
                </div>
            </div>


            <% var storedFiles = OnlineEditorsExample.DefaultConfigTools.GetStoredFiles();
               if (storedFiles.Any())
               { %>
            <div class="help-block">
                <span>Your documents</span>
                <br />
                <br />
                <div class="stored-list">
                    <table width="100%" cellspacing="0" cellpadding="0">
                        <thead>
                            <tr class="tableHeader">
                                <td class="tableHeaderCell tableHeaderCellFileName">Filename</td>
                                <td colspan="6" class="tableHeaderCell contentCells-shift">Editors</td>
                                <td colspan="3" class="tableHeaderCell">Viewers</td>
                            </tr>
                        </thead>
                        <tbody>
                            <% foreach (var storedFile in storedFiles)
                               { %>
                                <%
                                    var editUrl = "doceditor.aspx?fileID=" + HttpUtility.UrlEncode(storedFile);
                                    var docType = OnlineEditorsExample.DefaultConfigTools.DocumentType(storedFile);
                                %>
                                <tr class="tableRow" title="<%= storedFile %>">
                                    <td class="contentCells">
                                        <a class="stored-edit <%= docType %>" href="<%= editUrl %>" target="_blank">
                                            <span title="<%= storedFile %>"><%= storedFile %></span>
                                        </a>
                                        <a href="<%= OnlineEditorsExample.DefaultConfigTools.VirtualPath + WebConfigurationManager.AppSettings["storage-path"] + storedFile %>">
                                            <img class="icon-download" src="app_themes/images/download-24.png" alt="Download" title="Download" />
                                        </a>
                                        <a class="delete-file" data-filename="<%= storedFile %>">
                                            <img class="icon-delete" src="app_themes/images/delete-24.png" alt="Delete" title="Delete" />
                                        </a>
                                    </td>

                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=desktop&editorsMode=edit" %>" target="_blank">
                                            <img src="app_themes/images/desktop-24.png" alt="Open in editor for full size screens" title="Open in editor for full size screens"/>
                                        </a>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=mobile&editorsMode=edit" %>" target="_blank">
                                            <img src="app_themes/images/mobile-24.png" alt="Open in editor for mobile devices" title="Open in editor for mobile devices"/>
                                        </a>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <% if (docType == "text") { %>
                                            <a href="<%= editUrl + "&editorsType=desktop&editorsMode=review" %>" target="_blank">
                                                <img src="app_themes/images/review-24.png" alt="Open in editor for review" title="Open in editor for review"/>
                                            </a>
                                        <% } else if (docType == "spreadsheet") { %>
                                            <a href="<%= editUrl + "&editorsType=desktop&editorsMode=filter" %>" target="_blank">
                                                <img src="app_themes/images/filter-24.png" alt="Open in editor without access to change the filter" title="Open in editor without access to change the filter" />
                                            </a>
                                        <% } %>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=desktop&editorsMode=comment" %>" target="_blank">
                                            <img src="app_themes/images/comment-24.png" alt="Open in editor for comment" title="Open in editor for comment"/>
                                        </a>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <% if (docType == "text") { %>
                                            <a href="<%= editUrl + "&editorsType=desktop&editorsMode=fillForms" %>" target="_blank">
                                                <img src="app_themes/images/fill-forms-24.png" alt="Open in editor for filling in forms" title="Open in editor for filling in forms"/>
                                            </a>
                                        <% } %>
                                    </td>
                                    <td class="contentCells contentCells-icon contentCells-shift">
                                        <% if (docType == "text") { %>
                                            <a href="<%= editUrl + "&editorsType=desktop&editorsMode=blockcontent" %>" target="_blank">
                                                <img src="app_themes/images/block-content-24.png" alt="Open in editor without content control modification" title="Open in editor without content control modification"/>
                                            </a>
                                        <% } %>
                                    </td>

                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=desktop&editorsMode=view" %>" target="_blank">
                                            <img src="app_themes/images/desktop-24.png" alt="Open in viewer for full size screens" title="Open in viewer for full size screens"/>
                                        </a>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=mobile&editorsMode=view" %>" target="_blank">
                                            <img src="app_themes/images/mobile-24.png" alt="Open in viewer for mobile devices" title="Open in viewer for mobile devices"/>
                                        </a>
                                    </td>
                                    <td class="contentCells contentCells-icon">
                                        <a href="<%= editUrl + "&editorsType=embedded&editorsMode=embedded" %>" target="_blank">
                                            <img src="app_themes/images/embeded-24.png" alt="Open in embedded mode" title="Open in embedded mode"/>
                                        </a>
                                    </td>
                                </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>
            <% } %>


    
    
        </div>

   
    </form>
</body>
</html>
