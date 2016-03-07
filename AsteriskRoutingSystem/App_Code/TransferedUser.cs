using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for TranseferdUser
/// </summary>
/// 
[Serializable]
public class TransferedUser
{
    public string name_user { get; set; }
    public string original_context { get; set; }
    public string original_asterisk { get; set; }
    public string current_asterisk { get; set; }
}
