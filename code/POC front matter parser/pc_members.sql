SELECT member.role, COUNT(*) 
	FROM member 
	WHERE member.role LIKE '%Program%' 
	GROUP BY member.role 
	ORDER BY COUNT(*) DESC;
	
SELECT COUNT(*) 
	FROM member 
	WHERE member.role LIKE '%Program%';
	
SELECT file.filename, member.* 
	FROM member
	LEFT JOIN section ON section.id = member.section_id 
	LEFT JOIN file ON section.file_id = file.id
	WHERE member.role LIKE '%Program%'