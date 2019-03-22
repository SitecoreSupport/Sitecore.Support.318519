using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Support.Shell.Controls.RichTextEditor.InsertLink
{
  public class InsertLinkForm: Sitecore.Shell.Controls.RichTextEditor.InsertLink.InsertLinkForm
  {
    protected override void OnOK([NotNull] object sender, [NotNull] EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");

      string result;
      string text;

      if (this.Tabs.Active == 0 || this.Tabs.Active == 2)
      {
        Item item = this.InternalLinkTreeview.GetSelectionItem();

        if (item != null)
        {
          Language lang = GetLanguageFromQuery();
          item = item.Database.GetItem(item.ID, lang, Data.Version.Latest);
        }

        if (item == null)
        {
          SheerResponse.Alert(Texts.PLEASE_SELECT_AN_ITEM);
          return;
        }

        text = item.GetUIDisplayName();

        if (item.Paths.IsMediaItem)
        {
          result = GetMediaUrl(item);
        }
        else if (!item.Paths.IsContentItem)
        {
          SheerResponse.Alert(Texts.PLEASE_SELECT_EITHER_A_CONTENT_OR_A_MEDIA_ITEM);
          return;
        }
        else
        {
          var options = new LinkUrlOptions();

          result = LinkManager.GetDynamicUrl(item, options);
        }
      }
      else
      {
        MediaItem item = this.MediaTreeview.GetSelectionItem();

        if (item != null)
        {
          Language lang = GetLanguageFromQuery();
          item = item.InnerItem.Database.GetItem(item.InnerItem.ID, lang, Data.Version.Latest);
        }
        
        if (item == null)
        {
          SheerResponse.Alert(Texts.PLEASE_SELECT_A_MEDIA_ITEM);
          return;
        }        

        text = item.DisplayName;

        result = GetMediaUrl(item);
      }

      if (this.Mode == "webedit")
      {
        SheerResponse.SetDialogValue(StringUtil.EscapeJavascriptString(result));
        Base_OnOK(sender, args);
      }
      else
      {
        SheerResponse.Eval(
          "scClose(" + StringUtil.EscapeJavascriptString(result) + "," + StringUtil.EscapeJavascriptString(text) + ")");
      }
    }

    private string GetMediaUrl([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      return MediaManager.GetMediaUrl(item, MediaUrlOptions.GetShellOptions());
    }

    protected virtual void Base_OnOK(object sender, EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");
      SheerResponse.CloseWindow();
    }

    protected virtual Language GetLanguageFromQuery()
    {
      string lang = Sitecore.Web.WebUtil.GetQueryString("la");
      Language result = Sitecore.Context.Language;
      if (!string.IsNullOrEmpty(lang))
      {
        result = Language.Parse(lang);
      }
      return result;
    }

    [UsedImplicitly]
    private void OnMediaTreeviewClicked()
    {
      this.SetUploadButtonAvailability();
    }

    private void SetUploadButtonAvailability()
    {
      if (this.Tabs.Active != 1)
      {
        SheerResponse.Eval("document.getElementById('BtnUpload').style.display='none';");
      }
      else
      {
        SheerResponse.Eval("document.getElementById('BtnUpload').style.display='';");
        Item selectionItem = this.MediaTreeview.GetSelectionItem();
        if ((selectionItem == null) || !selectionItem.Access.CanCreate())
        {
          SheerResponse.Eval("document.getElementById('BtnUpload').disabled = true;");
        }
        else
        {
          SheerResponse.Eval("document.getElementById('BtnUpload').disabled = false;");
        }
      }
    }

  }
}