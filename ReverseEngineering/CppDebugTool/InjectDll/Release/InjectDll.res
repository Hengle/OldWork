        ��  ��                  3  ,   �� U I . H T M         0         <HTML XMLNS:sdk>
	<HEAD>
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<STYLE>
			sdk\:cacher {behavior:url(#default#userData);}
		</STYLE>
		<style>
			INPUT
			{
				width:100%;font-size:14px;
			}
			TABLE
			{
				font-size:14px;
			}
			.Edit
			{
				background-color: #FFFFFF;
				BORDER-RIGHT: #D0D0D0 1px solid; BORDER-TOP:  #D0D0D0 1px solid;BORDER-LEFT:  #D0D0D0 1px solid; BORDER-BOTTOM:  #D0D0D0 1px solid
			}
			.MouseOnEdit
			{
				background-color: #FFFFFF;
				BORDER-RIGHT: #A1A1A1 1px solid; BORDER-TOP: #A1A1A1 1px solid;BORDER-LEFT: #A1A1A1 1px solid; BORDER-BOTTOM: #A1A1A1 1px solid
			}
			.Result
			{			
				position:relative;overflow-y:scroll;
				left:0px;top:0px;width:100%;height:290px;
				SCROLLBAR-FACE-COLOR: #E8E8E8;
				SCROLLBAR-HIGHLIGHT-COLOR: #FFFFFF;
				SCROLLBAR-SHADOW-COLOR: #DCFFB9;
				SCROLLBAR-3DLIGHT-COLOR: #58BE1A;
				SCROLLBAR-ARROW-COLOR: #000000;
				SCROLLBAR-TRACK-COLOR: #EEEEEE;
				SCROLLBAR-DARKSHADOW-COLOR: #58BE1A
				background-color: #EEEEEE;
				BORDER-RIGHT: #ECC54F 1px solid; BORDER-TOP:  #ECC54F 1px solid;BORDER-LEFT:  #ECC54F 1px solid; BORDER-BOTTOM:  #ECC54F 1px solid
			}
			.ClickButton1 
			{ 
				font-size:14px; 
				cursor:hand; 
				background-color:#E8E8E8; 
				width:100px; height:24px; 
				BORDER-BOTTOM: buttonshadow 1px solid; BORDER-LEFT: buttonhighlight 1px solid; 
				BORDER-RIGHT: buttonshadow 1px solid; BORDER-TOP: buttonhighlight 1px solid; 
			}
			.ClickButton2 
			{ 
				font-size:14px; 
				cursor:hand; 
				background-color:#E8E8E8; 
				width:100px; height:24px; 
				BORDER-BOTTOM: buttonshadow 1px solid; BORDER-LEFT: buttonhighlight 1px solid; 
				BORDER-RIGHT: buttonshadow 1px solid; BORDER-TOP: buttonhighlight 1px solid; 
			}
		</style>
	</head>
	<body style="BACKGROUND-COLOR: #FFFFFF" topmargin="0" leftmargin="0" rightmargin="0" scroll="no">
		<sdk:cacher id="cachetag"></sdk:cacher>
		<table style="BORDER-COLLAPSE: collapse;width:98%;height:100%" cellpadding="0" cellspacing="0" border="0" align="center" ID="Table1">
			<tr height="24">
				<td>
					<table style="table-layout:fixed;BORDER-COLLAPSE: collapse;width:100%;height:100%;font-size:14px" cellpadding="0" cellspacing="0" border="0" ID="Table2">
						<tr>							
							<td style="width:90px">
								������������
							</td>
							<td>
								<INPUT id="procName" type="text" class="edit" value="Init" title="ע��DLL�ĳ�ʼ����������������DLL������޲����Ĺ���" onmouseover="this.className='MouseOnEdit'" onmouseout="this.className='Edit'" onblur="this.className='Edit'" NAME="procName">
							</td>
							<td style="width:60px">
								&nbsp;&nbsp;������
							</td>
							<td>
								<INPUT id="procArgs" type="text" class="edit">
							</td>
							<td style="width:60px;">
								&nbsp;&nbsp;���ԣ�
							</td>
							<td style="width:20px;border:solid 1px #D0D0D0;">
								<INPUT id="debugState" type="checkbox">
							</td>
							<td style="width:60px;">
								&nbsp;&nbsp;�ȴ���
							</td>
							<td style="width:20px;border:solid 1px #D0D0D0;">
								<INPUT id="waitState" type="checkbox">
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr height="24">
				<td>
					<table style="table-layout:fixed;BORDER-COLLAPSE: collapse;width:100%;height:100%;font-size:14px" cellpadding="0" cellspacing="0" border="0" ID="Table3">
						<tr>
							<td style="width:90px">
								ע  ��  DLL��
							</td>
							<td>
								<INPUT id="dllPath" type="text" class="edit" readonly tabindex="-1" title="Ҫע��Ŀ����̵�DLL�ļ���˫����ѡ��һ��DLL�ļ�" onmouseover="this.className='MouseOnEdit'" onmouseout="this.className='Edit'" onblur="this.className='Edit'" ondblclick="selDll()" NAME="dllPath">
							</td>
							<td style="width:90px">
								&nbsp;&nbsp;ѡ�н��̣�
							</td>
							<td style="width:120px">
								<INPUT id="procFile" type="text" readonly tabindex="-1" class="edit" NAME="procFile">
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr>
				<td valign="top">
					<div class="Result" id="ttt">
						<table id="procList" style="table-layout:fixed;BORDER-COLLAPSE: collapse;width:100%;display:;" border="1" bordercolor="#D0D0D0" cellpadding="2" cellspacing="0" onmouseover="tableOver()" onmouseout="tableOut()" onmousedown="tableDown()">
						</table>
					</div>
				</td>
			</tr>
			<tr height="2px">
				<td>
				</td>
			</tr>
			<tr height="24">
				<td>
					<table style="BORDER-COLLAPSE: collapse;width:100%;height:100%;font-size:14px" cellpadding="0" cellspacing="0" border="0" ID="Table4">
						<tr>
							<td style="width:90px">							
							</td>
							<td>								
								<input type="button" class="ClickButton2" onclick="injectHook();" title="��ǰѡ�еĽ���ע��DLL������DLL�е���������" value="ע��DLL������" ID="Button1" NAME="Button1">
							</td>
							<td>
								<input type="button" class="ClickButton2" onclick="freeDll();" title="�ͷŵ�ǰѡ�еĽ��̿ռ����ָ��DLL������ִ��һ��FreeLibrary" value=" �� �� DLL " ID="Button2" NAME="Button2">
							</td>
							<td>
								<input type="button" class="ClickButton2" onclick="refreshProcs();" title="ˢ�½����б�" value="ˢ �� �� ��" ID="Button3" NAME="Button3">
							</td>
							<td>
								<input type="button" class="ClickButton2" onclick="runProg();" title="����һ�����򣬲��ڳ�������ǰִ��ָ�����еĺ���" value="�� �� �� ��" ID="Button4" NAME="Button3">
							</td>
							<td>
								<input type="button" class="ClickButton2" onclick="runProg2();" title="����һ�����򣬲��ڳ�������ǰִ��ָ�����еĺ���" value="���г���RT" ID="Button5" NAME="Button3">
							</td>
						</tr>
					</table>
				</td>
			</tr>		
			<tr height="2px">
				<td>
				</td>
			</tr>
		</table>
		<script language="javascript">
			var selProcID=0;
			function window.onload()
			{
				try
				{					
					window.external.Move(-1,-1,640,480);
					window.external.Center();
					window.external.Application.Initialize();
				}
				catch(e)
				{				
				}
				cachetag.load("InjectDll");
				var v=cachetag.getAttribute("procName");
				if(v && v.length>0)
					procName.value=v;
				v=cachetag.getAttribute("dllPath");
				if(v && v.length>0)
					dllPath.value=v;
			}
			function selDll()
			{
				var r=window.external.SelFile(1,"","*.dll","��̬���ӿ�|*.dll||");
				if(r)
				{
					dllPath.value=r;
				}
			}						
			function injectHook()
			{			
				if(dllPath.value=="")
					dllPath.value=window.external.SelFile(1,"","*.dll","��̬���ӿ�|*.dll||");
				if(dllPath.value=="")
					return;				
				if(!selProcID)
				{
					alert("��ѡ��һ��Ŀ�����!");
					return;	
				}
				if(!window.external.Shell.ProcessExists(selProcID))
				{
					refreshProcs();
					selProcID=0;
					procFile.value="";
					alert("Ŀ������Ѿ��˳���������ѡ��Ŀ����̣�");
					return;
				}
				window.external.Application.InjectDll(selProcID,dllPath.value,debugState.checked);
				var flag=0;
				if(debugState.checked)
					flag|=0x01;
				if(waitState.checked)
					flag|=0x02;					
				var addr=window.external.HexToInt(procName.value);
				if(procName.value.length>0 && (procName.value.charCodeAt(0)<'0'.charCodeAt(0) || procName.value.charCodeAt(0)>'9'.charCodeAt(0)))
					addr=0;
				if(procArgs.value.length<=0)
				{
					var dllHandle = window.external.Application.GetDll(selProcID,dllPath.value,debugState.checked);
					if(addr==0)		
					{			
						addr=window.external.Application.GetProc(selProcID,dllHandle,procName.value,debugState.checked);
					}
					else
					{
						addr+=dllHandle;
					}
					window.external.Application.CallAddr(selProcID,addr,flag);
				}
				else
				{
					var args=eval("["+procArgs.value+"]");
					var dllHandle = window.external.Application.GetDll(selProcID,dllPath.value,debugState.checked);
					if(addr==0)	
					{
						addr=window.external.Application.GetProc(selProcID,dllHandle,procName.value,debugState.checked);
					}
					else
					{
						addr+=dllHandle;
					}
					window.external.Application.CallAddr2(selProcID,addr,flag,args);
				}
			}			
			function freeDll()
			{
				if(dllPath.value=="")
					dllPath.value=window.external.SelFile(1,"","*.dll","��̬���ӿ�|*.dll||");
				if(dllPath.value=="")
					return;
				if(!selProcID)
				{
					alert("��ѡ��һ��Ŀ�����!");
					return;	
				}
				if(!window.external.Shell.ProcessExists(selProcID))
				{
					refreshProcs();
					selProcID=0;
					procFile.value="";
					alert("Ŀ������Ѿ��˳���������ѡ��Ŀ����̣�");
					return;
				}
				window.external.Application.FreeDll(selProcID,dllPath.value,debugState.checked);
			}
			function refreshProcs()
			{
				while(procList.rows.length>0)
					procList.deleteRow();
				window.external.Application.RefreshProcessList();
			}
			function runProg()
			{
				if(dllPath.value=="")
					dllPath.value=window.external.SelFile(1,"","*.dll","��̬���ӿ�|*.dll||");
				if(dllPath.value=="")
					return;
				var r=window.external.SelFile(1,"","*.exe","��ִ�г���|*.exe||");
				if(!r || r=="")
					return;
				var addr=window.external.HexToInt(procName.value);
				if(procName.value.length>0 && (procName.value.charCodeAt(0)<'0'.charCodeAt(0) || procName.value.charCodeAt(0)>'9'.charCodeAt(0)))
					addr=0;
				if(procArgs.value.length<=0)
				{
					if(addr==0)
						window.external.Application.RunProgram(r,dllPath.value,procName.value,0,debugState.checked);
					else
						window.external.Application.RunProgram(r,dllPath.value,addr,0,debugState.checked);
				}
				else
				{
					var args=eval("["+procArgs.value+"]");					
					if(addr==0)
						window.external.Application.RunProgram2(r,dllPath.value,procName.value,0,debugState.checked,args);
					else
						window.external.Application.RunProgram2(r,dllPath.value,addr,0,debugState.checked,args);
				}
			}
			function runProg2()
			{
				if(dllPath.value=="")
					dllPath.value=window.external.SelFile(1,"","*.dll","��̬���ӿ�|*.dll||");
				if(dllPath.value=="")
					return;
				var r=window.external.SelFile(1,"","*.exe","��ִ�г���|*.exe||");
				if(!r || r=="")
					return;
				var flag=0;
				if(debugState.checked)
					flag|=0x01;
				if(waitState.checked)
					flag|=0x02;	
				var addr=window.external.HexToInt(procName.value);
				if(procName.value.length>0 && (procName.value.charCodeAt(0)<'0'.charCodeAt(0) || procName.value.charCodeAt(0)>'9'.charCodeAt(0)))
					addr=0;
				if(procArgs.value.length<=0)
				{
					if(addr==0)
						window.external.Application.RunProgramRT(r,dllPath.value,procName.value,0,flag);
					else
						window.external.Application.RunProgramRT(r,dllPath.value,addr,0,flag);
				}
				else
				{
					var args=eval("["+procArgs.value+"]");					
					if(addr==0)
						window.external.Application.RunProgramRT2(r,dllPath.value,procName.value,0,flag,args);
					else
						window.external.Application.RunProgramRT2(r,dllPath.value,addr,0,flag,args);
				}
			}
		</script>
		<script>
			//---------------------------------------------------------------------------
			var curTR=null;
			function tableOver()
			{
				var srcObj=event.srcElement;
				if(srcObj.tagName=="TD")
				{
					srcObj=srcObj.parentElement;
					if(srcObj!=curTR)
					{
						srcObj.bgColor="#E8E8E8";
					}
				}
			}
			function tableOut()
			{
				var srcObj=event.srcElement;
				if(srcObj.tagName=="TD")
				{
					srcObj=srcObj.parentElement;
					if(srcObj!=curTR)
					{
						srcObj.bgColor="#FFFFFF";
					}
				}
			}
			function tableDown()
			{
				var srcObj=event.srcElement;
				if(srcObj.tagName=="TD")
				{
					srcObj=srcObj.parentElement;
					if(curTR!=srcObj)
					{
						if(curTR)
							curTR.bgColor="#FFFFFF";
						curTR=srcObj;
					}
					srcObj.bgColor="#EBBD32";
					selProcID=srcObj.procID;
					procFile.value=srcObj.procFile;
				}
			}
			//---------------------------------------------------------------------------		
			function OnClose()
			{
				if(procName.value.length>0)
					cachetag.setAttribute("procName",procName.value);
				if(dllPath.value.length>0)
					cachetag.setAttribute("dllPath",dllPath.value);
				cachetag.save("InjectDll");
			}	
			function OnSize()
			{					
				ttt.style.height=document.body.clientHeight-76;
			}
			function PutProcInfo(pid,file,path)
			{
				var tr=procList.insertRow();
				tr.procID=pid;
				tr.procFile=file;
				var cell=tr.insertCell();
				cell.innerText="ID:"+pid;
				cell.style.pixelWidth=80;
				cell.style.color="#E143BF";
				cell=tr.insertCell();
				cell.innerText=file;
				cell.style.pixelWidth=120;
				cell.style.color="#0343BF";
				cell=tr.insertCell();
				cell.innerText=path;
				cell.style.color="#32C54F";
			}
		</script>
	</body>
</html>
�      �� ��     	        (       @                                �  �   �� �   � � ��  ��� ���   �  �   �� �   � � ��  ��� ����������������������������������������������������������������������������������������ϙ��������������ϙ��������������ɟ������������������������������������������������������������������������������ɟ��������������ϙ��������������ϙ��������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������                                                                                                                                (      �� ��     	        (                �                       �  �   �� �   � � ��  ��� ���   �  �   �� �   � � ��  ��� ����������������������������ə������������������������������ə������������������������������������������������������������������                                                                "       �� ���     0	                 �      (     p       ��	 ���     0 	        	 N  �  	 O �  	 S �  	 P �  	 Z +�  	 X #�  	 C "�  	 V %�    +�   . #�  	 - "�   - %�   u P�  � u Q�  �      �� ��     0 	        �4   V S _ V E R S I O N _ I N F O     ���               ?                         F   S t r i n g F i l e I n f o   "   0 4 0 9 0 4 b 0   8   C o m p a n y N a m e     N a t i o n p h o n e   `   F i l e D e s c r i p t i o n     i n j e c t   d l l   t o   o t h e r   p r o c e s s   6   F i l e V e r s i o n     0 ,   1 ,   0 ,   2     4 
  I n t e r n a l N a m e   I n j e c t D l l   B   L e g a l C o p y r i g h t   C o p y r i g h t   2 0 0 6     D   O r i g i n a l F i l e n a m e   I n j e c t D l l . e x e   B   P r o d u c t N a m e     I n j e c t D l l   M o d u l e     :   P r o d u c t V e r s i o n   0 ,   1 ,   0 ,   2     D    V a r F i l e I n f o     $    T r a n s l a t i o n     	�P      �� ��     0 	        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
<assemblyIdentity 
	version="1.0.0.0" 
	processorArchitecture="x86" 
	name="InjectDll" 
	type="win32" 
/> 
<description>
	InjectDll Application
</description>
<dependency>
	<dependentAssembly>
		<assemblyIdentity 
			type="win32" 
			name="Microsoft.Windows.Common-Controls" 
			version="6.0.0.0" 
			processorArchitecture="x86" 
			publicKeyToken="6595b64144ccf1df" 
			language="*" 
		/>
	</dependentAssembly>
</dependency>
</assembly>
2       �� ��	     0	        	 I n j e c t D l l                                 �      �� ��    0	         C r e a t e   a   n e w   d o c u m e n t 
 N e w  O p e n   a n   e x i s t i n g   d o c u m e n t 
 O p e n  C l o s e   t h e   a c t i v e   d o c u m e n t 
 C l o s e  S a v e   t h e   a c t i v e   d o c u m e n t 
 S a v e 0 S a v e   t h e   a c t i v e   d o c u m e n t   w i t h   a   n e w   n a m e 
 S a v e   A s & C h a n g e   t h e   p r i n t i n g   o p t i o n s 
 P a g e   S e t u p 3 C h a n g e   t h e   p r i n t e r   a n d   p r i n t i n g   o p t i o n s 
 P r i n t   S e t u p  P r i n t   t h e   a c t i v e   d o c u m e n t 
 P r i n t     D i s p l a y   f u l l   p a g e s 
 P r i n t   P r e v i e w                     �� ��    0	        ? D i s p l a y   p r o g r a m   i n f o r m a t i o n ,   v e r s i o n   n u m b e r   a n d   c o p y r i g h t 
 A b o u t 4 Q u i t   t h e   a p p l i c a t i o n ;   p r o m p t s   t o   s a v e   d o c u m e n t s 
 E x i t                               �       �� ��    0	        ( S w i t c h   t o   t h e   n e x t   w i n d o w   p a n e 
 N e x t   P a n e 5 S w i t c h   b a c k   t o   t h e   p r e v i o u s   w i n d o w   p a n e 
 P r e v i o u s   P a n e                               |      �� ��    0	        6 O p e n   a n o t h e r   w i n d o w   f o r   t h e   a c t i v e   d o c u m e n t 
 N e w   W i n d o w 7 A r r a n g e   i c o n s   a t   t h e   b o t t o m   o f   t h e   w i n d o w 
 A r r a n g e   I c o n s / A r r a n g e   w i n d o w s   s o   t h e y   o v e r l a p 
 C a s c a d e   W i n d o w s 5 A r r a n g e   w i n d o w s   a s   n o n - o v e r l a p p i n g   t i l e s 
 T i l e   W i n d o w s 5 A r r a n g e   w i n d o w s   a s   n o n - o v e r l a p p i n g   t i l e s 
 T i l e   W i n d o w s ( S p l i t   t h e   a c t i v e   w i n d o w   i n t o   p a n e s 
 S p l i t                     (      �� ��    0	         E r a s e   t h e   s e l e c t i o n 
 E r a s e  E r a s e   e v e r y t h i n g 
 E r a s e   A l l 3 C o p y   t h e   s e l e c t i o n   a n d   p u t   i t   o n   t h e   C l i p b o a r d 
 C o p y 1 C u t   t h e   s e l e c t i o n   a n d   p u t   i t   o n   t h e   C l i p b o a r d 
 C u t  F i n d   t h e   s p e c i f i e d   t e x t 
 F i n d  I n s e r t   C l i p b o a r d   c o n t e n t s 
 P a s t e      R e p e a t   t h e   l a s t   a c t i o n 
 R e p e a t 1 R e p l a c e   s p e c i f i c   t e x t   w i t h   d i f f e r e n t   t e x t 
 R e p l a c e % S e l e c t   t h e   e n t i r e   d o c u m e n t 
 S e l e c t   A l l  U n d o   t h e   l a s t   a c t i o n 
 U n d o & R e d o   t h e   p r e v i o u s l y   u n d o n e   a c t i o n 
 R e d o       �      �� ���    0	         C h a n g e   t h e   w i n d o w   s i z e  C h a n g e   t h e   w i n d o w   p o s i t i o n  R e d u c e   t h e   w i n d o w   t o   a n   i c o n  E n l a r g e   t h e   w i n d o w   t o   f u l l   s i z e " S w i t c h   t o   t h e   n e x t   d o c u m e n t   w i n d o w & S w i t c h   t o   t h e   p r e v i o u s   d o c u m e n t   w i n d o w 9 C l o s e   t h e   a c t i v e   w i n d o w   a n d   p r o m p t s   t o   s a v e   t h e   d o c u m e n t s                   �       �� ���    0	            ! R e s t o r e   t h e   w i n d o w   t o   n o r m a l   s i z e  A c t i v a t e   T a s k   L i s t                        A c t i v a t e   t h i s   w i n d o w   *       �� ��    0	           R e a d y                               D       �� ���    0	                             O p e n   t h i s   d o c u m e n t           