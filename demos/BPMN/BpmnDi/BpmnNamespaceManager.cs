/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{

  /// <summary>
  /// Provides convenience methods to search for specific XElements and XAttributes and test results for the
  /// relevant BPMN Namespaces
  /// </summary>
  internal class BpmnNamespaceManager
  {
    private static XmlNamespaceManager NamespaceManager { get; set; }

    internal static XNamespace Xsi { get; private set; }
    internal static XNamespace Bpmn { get; private set; }
    internal static XNamespace BpmnDi { get; private set; }
    internal static XNamespace Di { get; private set; }
    internal static XNamespace Dc { get; private set; }

    static BpmnNamespaceManager() {
      
      Xsi = "http://www.w3.org/2001/XMLSchema-instance";
      Di = "http://www.omg.org/spec/DD/20100524/DI";
      Dc = "http://www.omg.org/spec/DD/20100524/DC";
      Bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
      BpmnDi = "http://www.omg.org/spec/BPMN/20100524/DI";
      
      NamespaceManager = new XmlNamespaceManager(new NameTable());
      NamespaceManager.AddNamespace("xmlns:xsi=","http://www.w3.org/2001/XMLSchema-instance");
      NamespaceManager.AddNamespace("xmlns:di", "http://www.omg.org/spec/BPMN/20100524/DI");
      NamespaceManager.AddNamespace("xmlns:dc","http://www.omg.org/spec/DD/20100524/DC" );
      NamespaceManager.AddNamespace("xmlns:bpmn","http://www.omg.org/spec/BPMN/20100524/MODEL");
    }


    /// <summary>
    /// Returns all Attributes in the list that belong to the given namespace
    /// </summary>
    /// <param name="list">The given list</param>
    /// <param name="nameSpace">The namespace</param>
    /// <returns>The list with all items left in the namespaces.</returns>
    internal static IEnumerable<XAttribute> AttributesInNamespace(IEnumerable<XAttribute> list, XNamespace nameSpace) {

      // Some Attributes do not have a namespace declared explicitly. Since we test the parent for the correct namespace this is ok.
      return list.Where(el => el.Name.Namespace.Equals(nameSpace) || el.Name.Namespace == "");
    }

    /// <summary>
    /// Returns the value of the given attribute in the given XElement
    /// </summary>
    /// <param name="xElement">The xElement</param>
    /// <param name="nameSpace">The namespace</param>
    /// <param name="attributeName">The local name of the attribute</param>
    /// <returns></returns>
    internal static string GetAttributeValue(XElement xElement, XNamespace nameSpace, string attributeName) {
      
      // Some Attributes do not have a namespace declared explicitly. Since we test the parent for the correct namespace this is ok.
      var attrList = xElement.Attributes(attributeName).Where(el => el.Name.NamespaceName == nameSpace || el.Name.Namespace == "");
      if (attrList.Any()) {
        return attrList.First().Value;
      }
      return null;
    }
    
    /// <summary>
    /// Returns the child XML element with the given namespace and local name
    /// </summary>
    /// <param name="xElement">The element</param>
    /// <param name="nameSpace">The namespace</param>
    /// <param name="localName">The local name</param>
    /// <returns></returns>
    internal static XElement GetElement(XElement xElement, XNamespace nameSpace, string localName) {
                    
      return xElement.Element(nameSpace + localName);
    }
    
    /// <summary>
    /// Returns the child XML element with the given namespace and local name
    /// </summary>
    /// <param name="xElement">The element</param>
    /// <param name="nameSpace">The namespace</param>
    /// <param name="localName">The local name</param>
    /// <returns></returns>
    internal static IEnumerable<XElement> GetElements(XElement xElement, XNamespace nameSpace, string localName) {
                    
      return xElement.Elements(nameSpace + localName);
    }
    
  }
}