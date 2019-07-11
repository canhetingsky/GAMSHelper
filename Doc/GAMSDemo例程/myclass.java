package TEST;

public class myclass {

	public static void main(String[] args) {
		String filePath = "C:\\Users\\Administrator\\Desktop\\GAMSHelper\\GAMSDemo\\GAMSDemo\\bin\\x64\\Release\\GAMSDemo.exe";
		String server = "DESKTOP-36C9L6T";
		String database = "lushushu";
		String user = "sa";
		String pwd = "123";
		String command1 = "\"select * from IMS_PATROL_PERSON_ON_DUTY;\"";
		String command2 = "\"select * from IMS_PATROL_TASK_SKILL;\"";
		String command3 = "\"select * from IMS_PATROL_PERSON_TASK_TIME;\"";
		String command4 = "\"select * from IMS_PATROL_TASK_SPEND_TIME;\"";
		String command5 = "\"select PERSON_ID from IMS_PATROL_PERSON_ON_DUTY where SKILL_LEVEL<=2;\"";
		Runtime rn = Runtime.getRuntime();
		Process p = null;
		try {
			String command = filePath+" "+server+" "+database+" "+user+" "+pwd+" "+command1+" "+command2+" "+command3+" "+command4;
			System.out.println("start");
			System.out.println("command:  "+command);
			p = rn.exec(command);
			System.out.println("......");
			p.waitFor();
			System.out.println("OK");
		} catch (Exception e) {
			System.out.println("Error!");
		}
	}

}
